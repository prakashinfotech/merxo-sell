import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  SellerService,
  CreateSellerProductDto,
  SellerVariantDto,
  UpsertVariantWithIdDto,
} from '../../../core/seller/seller.service';
import { CategoryService } from '../../../core/category/category.service';
import { MediaService } from '../../../core/media/media.service';
import { DialogService } from '../../../core/dialog/dialog.service';

interface CategoryNode {
  categoryId: number;
  name: string;
  parentCategoryId?: number;
  isFashion?: boolean;
  children?: CategoryNode[];
}

interface SizeVariant {
  variantId: number | null;
  size: string;
  sku: string;
  stock: number;
  priceDelta: number;
  isDefault: boolean;
  isActive: boolean;
}

interface ColorGroup {
  color: string;
  imageUrls: string[];
  uploading?: boolean;
  sizes: SizeVariant[];
}

@Component({
  selector: 'app-seller-product-form',
  standalone: false,
  templateUrl: './seller-product-form.html',
  styleUrls: ['./seller-product-form.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerProductForm implements OnInit {
  form!: FormGroup;
  isEdit = false;
  productId?: number;

  loading = true;
  saving = false;
  error = '';

  imageSource: 'url' | 'upload' = 'url';
  uploadingFile = false;

  rootCategories: CategoryNode[] = [];
  subCategories: CategoryNode[] = [];


  primaryImageUrl = '';
  secondaryImages: string[] = [];
  status = 'New';

  // ─── Variant builder state ──────────────────────────────────────────────
  colorGroups: ColorGroup[] = [];
  productColors: string[] = [];
  newColor = '';
  savingVariants = false;
  variantsDirty = false;
  variantsError = '';

  readonly suggestedColors = ['Red', 'Blue', 'Black', 'White', 'Green', 'Yellow', 'Pink', 'Grey', 'Beige'];
  readonly suggestedSizes  = ['XS', 'S', 'M', 'L', 'XL', 'XXL'];

  constructor(
    private fb: FormBuilder,
    private sellerService: SellerService,
    private categoryService: CategoryService,
    private mediaService: MediaService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private dialogService: DialogService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(300)]],
      description: ['', [Validators.maxLength(2000)]],
      rootCategoryId: [null, [Validators.required]],
      categoryId: [null],

      basePrice: [0, [Validators.required, Validators.min(0.01)]],
      salePrice: [null],
      stock: [0, [Validators.required, Validators.min(0)]],
      primaryImageUrl: ['', [Validators.required]],
    });
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.uploadingFile = true;
      this.cdr.markForCheck();
      this.mediaService.uploadFile(file).subscribe({
        next: (res) => {
          this.form.patchValue({ primaryImageUrl: res.url });
          this.uploadingFile = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.dialogService.notify('Upload Failed', 'There was an error uploading your image. Please try again.');
          this.uploadingFile = false;
          this.cdr.markForCheck();
        }
      });
    }
  }

  onSecondaryFilesSelected(event: any): void {
    const files: FileList = event.target.files;
    if (files.length > 0) {
      this.uploadingFile = true;
      this.cdr.markForCheck();

      const uploads = Array.from(files).map(file => this.mediaService.uploadFile(file));
      forkJoin(uploads).subscribe({
        next: (results) => {
          this.secondaryImages.push(...results.map(r => r.url));
          this.uploadingFile = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.dialogService.notify('Upload Failed', 'Some of your secondary images could not be uploaded.');
          this.uploadingFile = false;
          this.cdr.markForCheck();
        }
      });
    }
  }

  removeSecondaryImage(index: number): void {
    this.secondaryImages.splice(index, 1);
    this.cdr.markForCheck();
  }

  secondaryUrl = '';
  addSecondaryUrl(): void {
    if (this.secondaryUrl && this.secondaryUrl.trim()) {
      this.secondaryImages.push(this.secondaryUrl.trim());
      this.secondaryUrl = '';
      this.cdr.markForCheck();
    }
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!id;
    this.productId = id ? +id : undefined;

    forkJoin({
      categories: this.categoryService.getTree(),
    }).subscribe({
      next: ({ categories }) => {
        this.rootCategories = (categories ?? []).map(c => ({
          categoryId: c.categoryId,
          name: c.name,
          parentCategoryId: c.parentCategoryId ?? undefined,
          isFashion: !!c.isFashion,
          children: (c.children ?? []).map(ch => ({
            categoryId: ch.categoryId,
            name: ch.name,
            parentCategoryId: ch.parentCategoryId ?? undefined,
            isFashion: !!ch.isFashion,
          })),
        }));


        if (this.isEdit) this.loadProduct();
        else { this.loading = false; this.cdr.markForCheck(); }
      },
      error: () => {
        this.error = 'Failed to load form data.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });

    // Update sub-category list whenever the root selection changes.
    // Coerce to number — native <select> emits a string.
    this.form.get('rootCategoryId')!.valueChanges.subscribe((raw: number | string | null) => {
      const rootId = raw == null || raw === '' ? null : Number(raw);
      const root = rootId != null ? this.rootCategories.find(r => r.categoryId === rootId) : undefined;
      this.subCategories = root?.children ?? [];

      const subRaw = this.form.get('categoryId')!.value;
      const subId  = subRaw == null || subRaw === '' ? null : Number(subRaw);
      const subStillValid = subId != null && this.subCategories.some(c => c.categoryId === subId);
      if (!subStillValid) this.form.patchValue({ categoryId: null }, { emitEvent: false });
      this.cdr.markForCheck();
    });

    // Sub-category drives the fashion flag in some taxonomies — keep the
    // template's isFashionCategory getter fresh on every change.
    this.form.get('categoryId')!.valueChanges.subscribe(() => this.cdr.markForCheck());
  }

  loadProduct(): void {
    if (!this.productId) return;
    this.sellerService.getProduct(this.productId).subscribe({
      next: p => {
        const detail = p as unknown as {
          categoryId: number;
          parentCategoryId?: number;
          name: string;
          description?: string;
          manufacturerId?: number;
          basePrice: number;
          salePrice?: number;
          stock: number;
          primaryImageUrl?: string;
          status: string;
        };

        const isRootCategory = !detail.parentCategoryId;
        this.form.patchValue({
          name: detail.name,
          description: detail.description ?? '',
          rootCategoryId: isRootCategory ? detail.categoryId : detail.parentCategoryId,

          basePrice: detail.basePrice,
          salePrice: detail.salePrice ?? null,
          stock: detail.stock,
          primaryImageUrl: detail.primaryImageUrl ?? (p as any).PrimaryImageUrl ?? '',
        });

        // Wait for the rootCategoryId valueChanges listener to populate subCategories,
        // then set the leaf selection.
        setTimeout(() => {
          if (!isRootCategory) {
            this.form.patchValue({ categoryId: detail.categoryId });
          }
          const images = (p as any).images || (p as any).Images || [];

          this.primaryImageUrl = detail.primaryImageUrl || (p as any).PrimaryImageUrl || '';
          this.secondaryImages = images
            .filter((img: any) => !(img.isPrimary || img.IsPrimary))
            .map((img: any) => img.imageUrl || img.ImageUrl);

          if (!this.primaryImageUrl && images.length > 0) {
            const primary = images.find((i: any) => i.isPrimary || i.IsPrimary);
            if (primary) this.primaryImageUrl = primary.imageUrl || primary.ImageUrl;
            else this.primaryImageUrl = images[0].imageUrl || images[0].ImageUrl;
          }

          this.form.patchValue({ primaryImageUrl: this.primaryImageUrl });

          this.status = detail.status;
          this.productColors = (p as any).colors || (p as any).Colors || [];

          this.loading = false;
          this.cdr.markForCheck();

          this.loadVariants();
        });
      },
      error: () => {
        this.error = 'Product not found or not yours.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  get hasSubCategories(): boolean {
    return this.subCategories.length > 0;
  }

  /**
   * Variant matrix is only meaningful for fashion-style categories.
   *
   * Note: native `<select>` writes its value as a string, so both ids are
   * coerced before lookup — otherwise `5 === "5"` silently fails and the
   * builder stays locked even on Fashion → Men.
   */
  get isFashionCategory(): boolean {
    const rootRaw = this.form.get('rootCategoryId')?.value;
    const subRaw  = this.form.get('categoryId')?.value;
    const rootId = rootRaw == null || rootRaw === '' ? null : Number(rootRaw);
    const subId  = subRaw  == null || subRaw  === '' ? null : Number(subRaw);
    if (rootId == null || Number.isNaN(rootId)) return false;

    const root = this.rootCategories.find(r => r.categoryId === rootId);
    if (!root) return false;
    if (root.isFashion) return true;

    if (subId == null || Number.isNaN(subId)) return false;
    const sub = (root.children ?? []).find(c => c.categoryId === subId);
    return !!sub?.isFashion;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = 'Please fill in all required fields.';
      this.cdr.markForCheck();
      return;
    }

    const v = this.form.value;
    const leafCategoryId = this.hasSubCategories && v.categoryId
      ? v.categoryId
      : v.rootCategoryId;

    if (this.hasSubCategories && !v.categoryId) {
      this.error = 'Please choose a sub-category.';
      this.cdr.markForCheck();
      return;
    }

    const dto: CreateSellerProductDto = {
      name: v.name.trim(),
      description: v.description?.trim() || undefined,
      categoryId: leafCategoryId,

      basePrice: +v.basePrice,
      salePrice: v.salePrice == null || v.salePrice === '' ? null : +v.salePrice,
      stock: +v.stock,
      images: [
        ...(v.primaryImageUrl ? [{ imageUrl: v.primaryImageUrl, isPrimary: true }] : []),
        ...this.secondaryImages.map(url => ({ imageUrl: url, isPrimary: false }))
      ],
      colors: this.colorGroups.map(g => g.color.trim()).filter(c => !!c),
      sizes: [...new Set(this.colorGroups.flatMap(g => g.sizes.map(s => s.size.trim())).filter(s => !!s))]
    };

    this.saving = true;
    this.error = '';

    const obs = this.isEdit && this.productId
      ? this.sellerService.updateProduct(this.productId, dto)
      : this.sellerService.createProduct(dto);

    obs.subscribe({
      next: (res: any) => {
        const savedId = res?.productId ?? this.productId;
        // Only re-persist variants when the user has explicitly changed variant data.
        // ⚠️ Do NOT use (this.isEdit && colorGroups.length > 0) here — that would
        // send empty imageUrls for colors whose images haven't been set yet, which
        // causes SyncVariantImages to call variant.Images.Clear() and wipe the DB.
        // The backend UpdateProduct now preserves variant-linked images correctly,
        // so no defensive re-persist is needed on a plain product info update.
        const shouldPersistVariants = savedId && this.variantsDirty;
        if (shouldPersistVariants) {
          this.persistVariants(savedId).subscribe({
            next: () => { this.saving = false; this.router.navigate(['/seller/products']); },
            error: () => { this.saving = false; this.router.navigate(['/seller/products']); },
          });
          return;
        }
        this.saving = false;
        this.router.navigate(['/seller/products']);
      },
      error: err => {
        this.saving = false;
        this.error = err?.error?.error || 'Save failed. Please try again.';
        this.cdr.markForCheck();
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/seller/products']);
  }

  // ─── Variant builder ────────────────────────────────────────────────────

  private loadVariants(): void {
    if (!this.productId) return;
    this.sellerService.getVariants(this.productId).subscribe({
      next: (rows: SellerVariantDto[]) => {
        this.colorGroups = [];

        if (rows && rows.length > 0) {
          const groupsMap = new Map<string, SellerVariantDto[]>();
          rows.forEach(r => {
            const colorKey = r.color || '';
            if (!groupsMap.has(colorKey)) {
              groupsMap.set(colorKey, []);
            }
            groupsMap.get(colorKey)!.push(r);
          });

          groupsMap.forEach((variants, color) => {
            const imageUrls = [...new Set(variants.flatMap(v => (v.images || []).map(i => i.imageUrl)))];
            this.colorGroups.push({
              color: color,
              imageUrls: imageUrls,
              sizes: variants.map(v => ({
                variantId: v.variantId,
                size: v.size || '',
                sku: v.sku || '',
                stock: v.stock,
                priceDelta: v.priceDelta,
                isActive: v.isActive,
                isDefault: v.isDefault
              }))
            });
          });

          // Remove variant-exclusive images from the main gallery
          const variantImageUrls = new Set(rows.flatMap(r => (r.images || []).map(i => i.imageUrl)));
          this.secondaryImages = this.secondaryImages.filter(url => !variantImageUrls.has(url));
        } else {
          // No variants yet: initialize colorGroups from productColors
          this.colorGroups = this.productColors.map(c => ({
            color: c,
            imageUrls: [],
            sizes: []
          }));
        }

        this.variantsDirty = false;
        this.cdr.markForCheck();
      },
      error: () => { /* product has no variants yet — fine */ }
    });
  }

  addColorGroup(colorName: string): void {
    const c = colorName.trim();
    if (!c) return;
    if (this.colorGroups.some(g => g.color.toLowerCase() === c.toLowerCase())) {
      this.dialogService.notify('Duplicate Color', 'This color option already exists.');
      return;
    }
    this.colorGroups.push({
      color: c,
      imageUrls: [],
      sizes: []
    });
    this.newColor = '';
    this.variantsDirty = true;
    this.cdr.markForCheck();
  }

  removeColorGroup(index: number): void {
    this.colorGroups.splice(index, 1);
    this.variantsDirty = true;
    this.cdr.markForCheck();
  }

  addSizeToGroup(groupIndex: number, sizeName: string): void {
    const s = sizeName.trim();
    if (!s) return;
    const group = this.colorGroups[groupIndex];
    if (group.sizes.some(x => x.size.toLowerCase() === s.toLowerCase())) {
      this.dialogService.notify('Duplicate Size', 'This size already exists for this color.');
      return;
    }
    group.sizes.push({
      variantId: null,
      size: s,
      sku: '',
      stock: 0,
      priceDelta: 0,
      isActive: true,
      isDefault: this.colorGroups.every(g => g.sizes.length === 0)
    });
    this.variantsDirty = true;
    this.cdr.markForCheck();
  }

  removeSizeFromGroup(groupIndex: number, sizeIndex: number): void {
    const group = this.colorGroups[groupIndex];
    const wasDefault = group.sizes[sizeIndex].isDefault;
    group.sizes.splice(sizeIndex, 1);

    if (wasDefault) {
      let set = false;
      for (const g of this.colorGroups) {
        if (g.sizes.length > 0) {
          g.sizes[0].isDefault = true;
          set = true;
          break;
        }
      }
    }
    this.variantsDirty = true;
    this.cdr.markForCheck();
  }

  markDefaultCombination(groupIndex: number, sizeIndex: number): void {
    this.colorGroups.forEach((g, gIdx) => {
      g.sizes.forEach((s, sIdx) => {
        s.isDefault = (gIdx === groupIndex && sIdx === sizeIndex);
      });
    });
    this.variantsDirty = true;
    this.cdr.markForCheck();
  }

  onVariantField(): void {
    this.variantsDirty = true;
    this.cdr.markForCheck();
  }

  onColorGroupImageUpload(groupIndex: number, event: any): void {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;
    const group = this.colorGroups[groupIndex];
    group.uploading = true;
    this.cdr.markForCheck();

    const uploads = Array.from(files).map(f => this.mediaService.uploadFile(f));
    forkJoin(uploads).subscribe({
      next: results => {
        group.imageUrls.push(...results.map(r => r.url));
        group.uploading = false;
        this.variantsDirty = true;
        this.cdr.markForCheck();
      },
      error: () => {
        group.uploading = false;
        this.dialogService.notify('Upload Failed', 'Color image upload failed.');
        this.cdr.markForCheck();
      }
    });
    event.target.value = '';
  }

  removeColorGroupImage(groupIndex: number, imageIndex: number): void {
    this.colorGroups[groupIndex].imageUrls.splice(imageIndex, 1);
    this.variantsDirty = true;
    this.cdr.markForCheck();
  }

  saveVariants(): void {
    if (!this.productId) {
      this.variantsError = 'Save the product first to add variants.';
      return;
    }
    this.savingVariants = true;
    this.variantsError = '';
    this.persistVariants(this.productId).subscribe({
      next: () => {
        this.savingVariants = false;
        this.variantsDirty = false;
        this.dialogService.notify('Variants Saved', 'Your variant configuration has been updated.');
        // Reload the full product to refresh secondaryImages, then reload variants
        // so that variant-linked images are correctly separated from the main gallery.
        this.loadProduct();
      },
      error: err => {
        this.savingVariants = false;
        this.variantsError = err?.error?.error || 'Could not save variants.';
        this.cdr.markForCheck();
      }
    });
  }

  private persistVariants(productId: number) {
    const payload: UpsertVariantWithIdDto[] = [];
    this.colorGroups.forEach(group => {
      group.sizes.forEach(sizeRow => {
        payload.push({
          variantId: sizeRow.variantId,
          color: group.color.trim() || null,
          size:  sizeRow.size.trim()  || null,
          sku:   sizeRow.sku.trim()   || null,
          priceDelta: +sizeRow.priceDelta || 0,
          stock: +sizeRow.stock || 0,
          isActive:  sizeRow.isActive,
          isDefault: sizeRow.isDefault,
          imageUrls: group.imageUrls,
        });
      });
    });
    return this.sellerService.saveVariants(productId, { variants: payload });
  }
}
