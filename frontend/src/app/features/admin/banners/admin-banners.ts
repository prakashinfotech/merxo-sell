import {
  Component, OnInit,
  ChangeDetectionStrategy, ChangeDetectorRef,
} from '@angular/core';
import {
  OfferBannerService,
  OfferBannerDto,
  CreateOfferBannerForm,
  OFFER_BANNER_SLOTS,
} from '../../../core/banner/banner.service';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-admin-banners',
  templateUrl: './admin-banners.html',
  styleUrls: ['./admin-banners.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminBanners implements OnInit {
  banners: OfferBannerDto[] = [];
  readonly slotOptions = OFFER_BANNER_SLOTS;

  loading  = true;
  saving   = false;
  uploading = false;
  uploadingSide = false;
  error    = '';

  showForm  = false;
  editing: OfferBannerDto | null = null;

  slotFilter   = 'all';
  statusFilter = 'all';

  form: CreateOfferBannerForm = this.emptyForm();

  // ── Derived ──────────────────────────────────────────────────────────────
  get filteredBanners(): OfferBannerDto[] {
    return this.banners.filter(b => {
      if (this.slotFilter !== 'all' && b.slot !== this.slotFilter) return false;
      switch (this.statusFilter) {
        case 'active':   return b.isActive;
        case 'inactive': return !b.isActive;
        default: return true;
      }
    });
  }

  get totalCount():    number { return this.banners.length; }
  get activeCount():   number { return this.banners.filter(b => b.isActive).length; }
  get heroCount():     number { return this.banners.filter(b => b.slot === 'Hero').length; }
  get midCount():      number { return this.banners.filter(b => b.slot === 'MidLeft' || b.slot === 'MidRight').length; }

  constructor(
    private api:   OfferBannerService,
    private toast: ToastService,
    private cdr:   ChangeDetectorRef,
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.cdr.markForCheck();
    this.api.getAll().subscribe({
      next: rows => {
        this.banners = rows;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Failed to load banners');
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  openCreate(): void {
    this.editing  = null;
    this.form     = this.emptyForm();
    this.error    = '';
    this.showForm = true;
  }

  openEdit(banner: OfferBannerDto): void {
    this.editing = banner;
    this.form = {
      slot:            banner.slot,
      title:           banner.title,
      subtitle:        banner.subtitle        ?? '',
      badgeText:       banner.badgeText       ?? '',
      imageUrl:        banner.imageUrl,
      sideImageUrl:    banner.sideImageUrl    ?? '',
      ctaLabel:        banner.ctaLabel        ?? '',
      ctaUrl:          banner.ctaUrl          ?? '',
      secondaryLabel:  banner.secondaryLabel  ?? '',
      secondaryUrl:    banner.secondaryUrl    ?? '',
      backgroundColor: banner.backgroundColor ?? '',
      textColor:       banner.textColor       ?? '',
      linkedProductId:  banner.linkedProductId  ?? null,
      linkedCategoryId: banner.linkedCategoryId ?? null,
      startsAt:        banner.startsAt ? this.toDatetimeLocal(banner.startsAt) : null,
      endsAt:          banner.endsAt   ? this.toDatetimeLocal(banner.endsAt)   : null,
      sortOrder:       banner.sortOrder,
      isActive:        banner.isActive,
    };
    this.error    = '';
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editing  = null;
    this.error    = '';
  }

  save(): void {
    this.error = this.validate();
    if (this.error) return;

    this.saving = true;
    const payload = this.toPayload();
    const req = this.editing
      ? this.api.update(this.editing.bannerId, payload)
      : this.api.create(payload);

    req.subscribe({
      next: () => {
        this.saving   = false;
        this.showForm = false;
        this.toast.success(this.editing ? 'Banner updated' : 'Banner created');
        this.load();
      },
      error: err => {
        this.error  = err?.error?.message || 'Could not save banner.';
        this.saving = false;
        this.cdr.markForCheck();
      },
    });
  }

  onImageFileChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.uploading = true;
    this.cdr.markForCheck();
    this.api.uploadImage(file).subscribe({
      next: url => {
        this.form.imageUrl = url;
        this.uploading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Image upload failed');
        this.uploading = false;
        this.cdr.markForCheck();
      },
    });
  }

  onSideImageFileChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.uploadingSide = true;
    this.cdr.markForCheck();
    this.api.uploadImage(file).subscribe({
      next: url => {
        this.form.sideImageUrl = url;
        this.uploadingSide = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Side image upload failed');
        this.uploadingSide = false;
        this.cdr.markForCheck();
      },
    });
  }

  toggle(banner: OfferBannerDto): void {
    this.api.setActive(banner.bannerId, !banner.isActive).subscribe({
      next: ({ isActive }) => {
        banner.isActive = isActive;
        this.toast.success(isActive ? 'Banner activated' : 'Banner deactivated');
        this.cdr.markForCheck();
      },
      error: () => this.toast.error('Toggle failed'),
    });
  }

  remove(banner: OfferBannerDto): void {
    if (!confirm(`Delete banner "${banner.title}"?`)) return;
    this.api.delete(banner.bannerId).subscribe({
      next: () => { this.toast.success('Banner deleted'); this.load(); },
      error: () => this.toast.error('Delete failed'),
    });
  }

  slotLabel(slot: string): string {
    const map: Record<string, string> = {
      Hero: 'Hero', MidLeft: 'Mid Left', MidRight: 'Mid Right', Strip: 'Strip',
    };
    return map[slot] ?? slot;
  }

  statusOf(b: OfferBannerDto): 'active' | 'inactive' | 'scheduled' | 'expired' {
    if (!b.isActive) return 'inactive';
    const now = Date.now();
    if (b.startsAt && new Date(b.startsAt).getTime() > now) return 'scheduled';
    if (b.endsAt   && new Date(b.endsAt).getTime()   < now) return 'expired';
    return 'active';
  }

  windowLabel(b: OfferBannerDto): string {
    const start = b.startsAt ? new Date(b.startsAt).toLocaleDateString() : 'Now';
    const end   = b.endsAt   ? new Date(b.endsAt).toLocaleDateString()   : 'Always';
    return `${start} → ${end}`;
  }

  // ── Private helpers ───────────────────────────────────────────────────────
  private validate(): string {
    if (!this.form.title?.trim())    return 'Title is required.';
    if (!this.form.slot)              return 'Slot is required.';
    if (!this.form.imageUrl?.trim()) return 'Banner image is required. Upload an image first.';
    if (this.form.startsAt && this.form.endsAt && this.form.endsAt <= this.form.startsAt)
      return 'End date must be after start date.';
    if (this.form.ctaUrl && !this.form.ctaUrl.startsWith('/') && !this.form.ctaUrl.startsWith('http'))
      return 'CTA URL must start with / or http.';
    return '';
  }

  private toPayload(): CreateOfferBannerForm {
    return {
      ...this.form,
      title:          this.form.title.trim(),
      subtitle:       this.form.subtitle?.trim()        || undefined,
      badgeText:      this.form.badgeText?.trim()       || undefined,
      imageUrl:       this.form.imageUrl.trim(),
      sideImageUrl:   this.form.sideImageUrl?.trim()    || undefined,
      ctaLabel:       this.form.ctaLabel?.trim()        || undefined,
      ctaUrl:         this.form.ctaUrl?.trim()          || undefined,
      secondaryLabel: this.form.secondaryLabel?.trim()  || undefined,
      secondaryUrl:   this.form.secondaryUrl?.trim()    || undefined,
      backgroundColor: this.form.backgroundColor?.trim() || undefined,
      textColor:      this.form.textColor?.trim()       || undefined,
      startsAt:       this.form.startsAt
                        ? new Date(this.form.startsAt).toISOString()
                        : null,
      endsAt:         this.form.endsAt
                        ? new Date(this.form.endsAt).toISOString()
                        : null,
    };
  }

  private emptyForm(): CreateOfferBannerForm {
    return {
      slot: 'Hero', title: '', subtitle: '', badgeText: '',
      imageUrl: '', sideImageUrl: '', ctaLabel: '', ctaUrl: '',
      secondaryLabel: '', secondaryUrl: '',
      backgroundColor: '', textColor: '',
      linkedProductId: null, linkedCategoryId: null,
      startsAt: null, endsAt: null, sortOrder: 0, isActive: true,
    };
  }

  private toDatetimeLocal(iso: string): string {
    return new Date(iso).toISOString().slice(0, 16);
  }
}
