import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProfileService, UserProfileDto, AddressCreateUpdateDto } from '../../core/profile/profile.service';
import { AuthService } from '../../core/auth/auth.service';
import { ReviewService, MyReviewDto } from '../../core/review/review.service';
import { OrderService, OrderDto, OrderItemDto, OrderStatusHistoryDto } from '../../core/order/order.service';
import { CurrencyService } from '../../core/currency/currency.service';
import { ActivatedRoute } from '@angular/router';
import { ConfirmDialog } from '../../shared/components/confirm-dialog/confirm-dialog';
import { DialogService } from '../../core/dialog/dialog.service';
import { BrowsingHistoryDto, BrowsingHistoryService } from '../../core/browsing-history/browsing-history.service';
import { ToastService } from '../../core/toast/toast.service';

export type ProfileTab = 'info' | 'orders' | 'reviews' | 'history' | 'addresses' | 'currency' | 'payment' | 'security';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.html',
  styleUrls: ['./profile.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Profile implements OnInit {
  profile: UserProfileDto | null = null;
  loading = true;
  activeTab: ProfileTab = 'info';
  submitting = false;

  // Profile Info Form
  profileForm: FormGroup;

  // Address
  addressForm: FormGroup;
  showAddressForm = false;
  editingAddressId: number | null = null;

  // Security
  passwordForm: FormGroup;

  // Payments
  paymentForm: FormGroup;
  showPaymentForm = false;
  paymentFormType: 'Card' | 'UPI' = 'Card';

  // Reviews
  reviews: MyReviewDto[] = [];
  loadingReviews = false;
  editingReviewId: number | null = null;
  reviewForm: FormGroup;

  // Browsing history
  history: BrowsingHistoryDto[] = [];
  loadingHistory = false;
  historyPage = 1;

  // Orders
  orders: OrderDto[] = [];
  loadingOrders = false;
  timelineOpen = false;
  timeline: OrderStatusHistoryDto[] = [];
  loadingTimeline = false;
  timelineForOrderId: number | null = null;
  reviewOpen = false;
  reviewItem: OrderItemDto | null = null;
  reviewRating = 5;
  reviewComment = '';
  reviewSubmitting = false;
  reviewError = '';

  // Currency & Country
  currencyForm: FormGroup;
  readonly currencies = [
    { code: 'CAD', label: 'CAD - Canadian Dollar' },
    { code: 'USD', label: 'USD - US Dollar' },
    { code: 'EUR', label: 'EUR - Euro' },
    { code: 'GBP', label: 'GBP - British Pound' },
    { code: 'INR', label: 'INR - Indian Rupee' },
  ];
  readonly savedCards = [
    { id: 'card-visa', brand: 'Visa', label: 'Visa ending in 4242', expiry: '08/29' },
    { id: 'card-mastercard', brand: 'Mastercard', label: 'Mastercard ending in 1881', expiry: '11/28' },
  ];
  readonly savedUpis = [
    { id: 'upi-main', label: 'yogesh@upi' },
    { id: 'upi-work', label: 'MerxoSell@okicici' },
  ];
  readonly countries = [
    'Canada', 'United States', 'United Kingdom', 'India', 'Australia', 'Germany', 'France'
  ];

  get cards() { return this.profile?.paymentMethods?.filter(pm => pm.type === 'Card') || []; }
  get upis() { return this.profile?.paymentMethods?.filter(pm => pm.type === 'UPI') || []; }

  readonly navItems: { tab: ProfileTab; icon: string; label: string }[] = [
    { tab: 'info',      icon: 'fa-user-edit',     label: 'Profile' },
    { tab: 'orders',    icon: 'fa-shopping-bag',  label: 'My Orders' },
    { tab: 'reviews',   icon: 'fa-star',          label: 'My Reviews' },
    { tab: 'history',   icon: 'fa-clock',         label: 'Browsing History' },
    { tab: 'addresses', icon: 'fa-map-marker-alt',label: 'Addresses' },
    { tab: 'currency',  icon: 'fa-globe',         label: 'Currency & Country' },
    { tab: 'payment',   icon: 'fa-credit-card',   label: 'Payment Methods' },
    { tab: 'security',  icon: 'fa-shield-alt',    label: 'Password & Security' },
  ];

  constructor(
    private profileService: ProfileService,
    public authService: AuthService,
    private reviewService: ReviewService,
    private orderService: OrderService,
    private historyService: BrowsingHistoryService,
    public currencyService: CurrencyService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private dialogService: DialogService,
    private toast: ToastService
  ) {
    this.profileForm = this.fb.group({
      fullName:          ['', [Validators.required, Validators.minLength(3)]],
      phone:             [''],
      preferredCurrency: ['CAD', Validators.required]
    });

    this.addressForm = this.fb.group({
      fullName:     ['', Validators.required],
      addressLine1: ['', Validators.required],
      addressLine2: [''],
      city:         ['', Validators.required],
      state:        ['', Validators.required],
      postalCode:   ['', Validators.required],
      country:      ['Canada', Validators.required],
      phone:        [''],
      isDefault:    [false]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword:     ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });

    this.reviewForm = this.fb.group({
      rating:  [5, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', Validators.maxLength(500)]
    });

    this.currencyForm = this.fb.group({
      preferredCurrency: ['CAD', Validators.required],
      country:           ['Canada', Validators.required]
    });

    this.paymentForm = this.fb.group({
      type:     ['Card', Validators.required],
      label:    ['', [Validators.required]], // Card Number or UPI ID
      subLabel: ['', [Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]], // Expiry Date (optional for UPI)
      isDefault: [false]
    });

    // Update validators based on type
    this.paymentForm.get('type')?.valueChanges.subscribe(type => {
      const label = this.paymentForm.get('label');
      const subLabel = this.paymentForm.get('subLabel');
      
      if (type === 'Card') {
        label?.setValidators([Validators.required, Validators.pattern(/^\d{16}$/)]);
        subLabel?.setValidators([Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]);
      } else {
        label?.setValidators([Validators.required, Validators.pattern(/^[a-zA-Z0-9.\-_]{2,256}@[a-zA-Z]{2,64}$/)]);
        subLabel?.clearValidators();
      }
      label?.updateValueAndValidity();
      subLabel?.updateValueAndValidity();
    });
  }

  private passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  ngOnInit(): void {
    this.loadProfile();
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.switchTab(params['tab'] as ProfileTab);
      }
      if (params['action'] === 'add-card') {
        this.openPaymentForm('Card');
      } else if (params['action'] === 'add-upi') {
        this.openPaymentForm('UPI');
      }
    });
  }

  switchTab(tab: ProfileTab): void {
    this.activeTab = tab;
    if (tab === 'reviews' && this.reviews.length === 0) this.loadReviews();
    if (tab === 'orders'  && this.orders.length  === 0) this.loadOrders();
    if (tab === 'history' && this.history.length === 0) this.loadHistory();
  }

  // ── Profile ────────────────────────────────────────────────────────────────
  loadProfile(): void {
    this.loading = true;
    this.profileService.getProfile().subscribe({
      next: (p: UserProfileDto) => {
        this.profile = p;
        this.profileForm.patchValue({ fullName: p.fullName, phone: p.phone, preferredCurrency: p.preferredCurrency });
        this.currencyForm.patchValue({ preferredCurrency: p.preferredCurrency, country: p.country });
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
  }

  updateProfile(): void {
    if (this.profileForm.invalid) return;
    this.submitting = true;
    this.profileService.updateProfile(this.profileForm.value).subscribe({
      next: (updated: UserProfileDto) => {
        this.profile = updated;
        this.toast.success('Profile updated successfully.');
        this.submitting = false;
        this.cdr.markForCheck();
      },
      error: () => { this.submitting = false; this.cdr.markForCheck(); }
    });
  }

  // ── Orders ─────────────────────────────────────────────────────────────────
  loadOrders(): void {
    this.loadingOrders = true;
    this.orderService.getUserOrders().subscribe({
      next: (data: OrderDto[]) => {
        this.orders = data;
        this.loadingOrders = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingOrders = false; this.cdr.markForCheck(); }
    });
  }

  // ── Reviews ────────────────────────────────────────────────────────────────
  loadReviews(): void {
    this.loadingReviews = true;
    this.reviewService.getMine().subscribe({
      next: (res: MyReviewDto[]) => {
        this.reviews = res;
        this.loadingReviews = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingReviews = false; this.cdr.markForCheck(); }
    });
  }

  startEditReview(review: MyReviewDto): void {
    this.editingReviewId = review.reviewId;
    this.reviewForm.patchValue({ rating: review.rating, comment: review.comment || '' });
  }

  cancelEditReview(): void {
    this.editingReviewId = null;
    this.reviewForm.reset({ rating: 5 });
  }

  saveReview(): void {
    if (this.reviewForm.invalid || !this.editingReviewId) return;
    this.submitting = true;
    this.reviewService.updateReview(this.editingReviewId, this.reviewForm.value).subscribe({
      next: () => { this.submitting = false; this.cancelEditReview(); this.loadReviews(); },
      error: () => { this.submitting = false; this.cdr.markForCheck(); }
    });
  }

  deleteReview(id: number): void {
    this.dialogService.ask({
      title: 'Delete Review',
      message: 'Are you sure you want to delete this review?',
      isDanger: true
    }).subscribe(res => {
      if (res) {
        this.reviewService.deleteReview(id).subscribe({ next: () => this.loadReviews() });
      }
    });
  }

  // ── Browsing History ──────────────────────────────────────────────────────
  historyFilter: 'all' | 'today' | 'yesterday' | 'week' | 'month' = 'all';

  get groupedHistory(): { id: string; title: string; icon: string; badgeClass: string; items: BrowsingHistoryDto[] }[] {
    if (!this.history || !this.history.length) return [];

    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const startOfYesterday = new Date(startOfToday);
    startOfYesterday.setDate(startOfYesterday.getDate() - 1);

    const startOfThisWeek = new Date(startOfToday);
    startOfThisWeek.setDate(startOfThisWeek.getDate() - 7);

    const startOfThisMonth = new Date(startOfToday);
    startOfThisMonth.setDate(startOfThisMonth.getDate() - 30);

    const todayItems: BrowsingHistoryDto[] = [];
    const yesterdayItems: BrowsingHistoryDto[] = [];
    const weekItems: BrowsingHistoryDto[] = [];
    const monthItems: BrowsingHistoryDto[] = [];
    const olderItems: BrowsingHistoryDto[] = [];

    for (const item of this.history) {
      const itemDate = new Date(item.viewedAt);
      if (itemDate >= startOfToday) {
        todayItems.push(item);
      } else if (itemDate >= startOfYesterday) {
        yesterdayItems.push(item);
      } else if (itemDate >= startOfThisWeek) {
        weekItems.push(item);
      } else if (itemDate >= startOfThisMonth) {
        monthItems.push(item);
      } else {
        olderItems.push(item);
      }
    }

    const allGroups = [
      { id: 'today', title: 'Today', icon: 'fa-calendar-day', badgeClass: 'badge-today', items: todayItems },
      { id: 'yesterday', title: 'Yesterday', icon: 'fa-history', badgeClass: 'badge-yesterday', items: yesterdayItems },
      { id: 'week', title: 'Last 7 Days', icon: 'fa-calendar-week', badgeClass: 'badge-week', items: weekItems },
      { id: 'month', title: 'Last 30 Days', icon: 'fa-calendar-alt', badgeClass: 'badge-month', items: monthItems },
      { id: 'older', title: 'Earlier', icon: 'fa-clock-rotate-left', badgeClass: 'badge-older', items: olderItems }
    ];

    const activeGroups = allGroups.filter(g => g.items.length > 0);

    if (this.historyFilter === 'all') {
      return activeGroups;
    }
    return activeGroups.filter(g => g.id === this.historyFilter);
  }

  setHistoryFilter(filter: 'all' | 'today' | 'yesterday' | 'week' | 'month'): void {
    this.historyFilter = filter;
    this.cdr.markForCheck();
  }

  loadHistory(page = 1): void {
    this.loadingHistory = true;
    this.historyService.get(page, 24).subscribe({
      next: rows => {
        this.historyPage = page;
        this.history = page === 1 ? rows : [...this.history, ...rows];
        this.loadingHistory = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingHistory = false; this.cdr.markForCheck(); }
    });
  }

  removeHistoryItem(id: number): void {
    this.historyService.delete(id).subscribe(() => {
      this.history = this.history.filter(h => h.browsingHistoryId !== id);
      this.cdr.markForCheck();
    });
  }

  clearHistory(): void {
    this.dialogService.ask({
      title: 'Clear Browsing History',
      message: 'Remove all recently viewed products from your profile?',
      isDanger: true
    }).subscribe(res => {
      if (!res) return;
      this.historyService.clear().subscribe(() => {
        this.history = [];
        this.cdr.markForCheck();
      });
    });
  }

  // ── Addresses ──────────────────────────────────────────────────────────────
  toggleAddressForm(addressId: number | null = null): void {
    this.editingAddressId = addressId;
    if (addressId) {
      const addr = this.profile?.addresses.find(a => a.addressId === addressId);
      if (addr) this.addressForm.patchValue(addr);
    } else {
      this.addressForm.reset({ country: 'Canada', isDefault: false });
    }
    this.showAddressForm = !this.showAddressForm;
    this.cdr.markForCheck();
  }

  saveAddress(): void {
    if (this.addressForm.invalid) return;
    this.submitting = true;
    const dto: AddressCreateUpdateDto = this.addressForm.value;
    const obs = this.editingAddressId
      ? this.profileService.updateAddress(this.editingAddressId, dto)
      : this.profileService.addAddress(dto);
    obs.subscribe({
      next: () => { this.submitting = false; this.showAddressForm = false; this.loadProfile(); },
      error: () => { this.submitting = false; this.cdr.markForCheck(); }
    });
  }

  deleteAddress(id: number): void {
    this.dialogService.ask({
      title: 'Delete Address',
      message: 'Are you sure you want to delete this address?',
      isDanger: true
    }).subscribe(res => {
      if (res) {
        this.profileService.deleteAddress(id).subscribe(() => this.loadProfile());
      }
    });
  }

  setDefaultAddress(id: number): void {
    this.profileService.setDefaultAddress(id).subscribe(() => this.loadProfile());
  }

  // ── Currency & Country ─────────────────────────────────────────────────────
  saveCurrency(): void {
    if (this.currencyForm.invalid) return;
    this.submitting = true;
    const val = this.currencyForm.value;
    this.profileService.updateProfile({
      fullName: this.profile?.fullName ?? '',
      phone: this.profile?.phone,
      preferredCurrency: val.preferredCurrency,
      country: val.country
    }).subscribe({
      next: (updated: UserProfileDto) => {
        this.profile = updated;
        this.profileForm.patchValue({ preferredCurrency: updated.preferredCurrency });
        this.currencyService.selectCurrency(updated.preferredCurrency);
        this.toast.success('Currency and country preferences updated.');
        this.submitting = false;
        this.cdr.markForCheck();
      },
      error: () => { this.submitting = false; this.cdr.markForCheck(); }
    });
  }

  // ── Payments ───────────────────────────────────────────────────────────────
  openPaymentForm(type: 'Card' | 'UPI'): void {
    this.paymentFormType = type;
    this.paymentForm.reset({ type, isDefault: false });
    this.showPaymentForm = true;
    this.cdr.markForCheck();
  }

  closePaymentForm(): void {
    this.showPaymentForm = false;
    this.cdr.markForCheck();
  }

  savePaymentMethod(): void {
    if (this.paymentForm.invalid) return;
    this.submitting = true;
    this.profileService.addPaymentMethod(this.paymentForm.value).subscribe({
      next: () => {
        this.submitting = false;
        this.showPaymentForm = false;
        this.loadProfile();
        this.toast.success('Payment method added.');
      },
      error: () => { this.submitting = false; this.cdr.markForCheck(); }
    });
  }

  deletePayment(id: number): void {
    this.dialogService.ask({
      title: 'Delete Payment Method',
      message: 'Are you sure you want to delete this payment method?',
      isDanger: true
    }).subscribe(res => {
      if (res) {
        this.profileService.deletePaymentMethod(id).subscribe(() => this.loadProfile());
      }
    });
  }

  setDefaultPayment(id: number): void {
    this.profileService.setDefaultPaymentMethod(id).subscribe(() => this.loadProfile());
  }

  // ── Security ───────────────────────────────────────────────────────────────
  updatePassword(): void {
    if (this.passwordForm.invalid) return;
    this.submitting = true;
    this.profileService.changePassword(this.passwordForm.value).subscribe({
      next: () => { this.submitting = false; this.passwordForm.reset(); this.cdr.markForCheck(); },
      error: (err: { error?: { error?: string } }) => {
        this.submitting = false;
        this.toast.error('Password change failed', err?.error?.error);
        this.cdr.markForCheck();
      }
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      Pending: 'status-pending', Processing: 'status-processing',
      Shipped: 'status-shipped', Delivered: 'status-delivered',
      Cancelled: 'status-cancelled'
    };
    return map[status] ?? 'status-pending';
  }

  stars(n: number): number[] { return Array.from({ length: n }, (_, i) => i + 1); }

  canReview(order: OrderDto): boolean {
    return (order.status ?? '').toLowerCase() === 'delivered';
  }

  openReviewModal(item: OrderItemDto): void {
    this.reviewItem = item;
    this.reviewRating = 5;
    this.reviewComment = '';
    this.reviewError = '';
    this.reviewOpen = true;
    this.cdr.markForCheck();
  }

  closeReviewModal(): void {
    this.reviewOpen = false;
    this.reviewItem = null;
    this.reviewError = '';
    this.cdr.markForCheck();
  }

  setReviewRating(rating: number): void {
    this.reviewRating = rating;
    this.cdr.markForCheck();
  }

  submitOrderReview(): void {
    if (!this.reviewItem) return;
    this.reviewSubmitting = true;
    this.reviewError = '';
    this.reviewService.addReview({
      productId: this.reviewItem.productId,
      rating: this.reviewRating,
      comment: this.reviewComment.trim() || undefined,
    }).subscribe({
      next: () => {
        this.toast.success(`Review posted for ${this.reviewItem!.productName}.`);
        this.reviewSubmitting = false;
        this.reviewOpen = false;
        this.reviewItem = null;
        this.reviews = [];
        this.cdr.markForCheck();
      },
      error: err => {
        this.reviewSubmitting = false;
        this.reviewError = err?.error?.error ?? 'Review could not be posted.';
        this.cdr.markForCheck();
      }
    });
  }

  openTimeline(order: OrderDto): void {
    this.timelineOpen = true;
    this.loadingTimeline = true;
    this.timeline = [];
    this.timelineForOrderId = order.orderId;
    this.cdr.markForCheck();
    this.orderService.getOrderHistory(order.orderId).subscribe({
      next: rows => {
        this.timeline = rows;
        this.loadingTimeline = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingTimeline = false;
        this.cdr.markForCheck();
      }
    });
  }

  closeTimeline(): void {
    this.timelineOpen = false;
    this.timeline = [];
    this.timelineForOrderId = null;
    this.cdr.markForCheck();
  }

  statusVariant(status: string): string {
    const s = (status || '').toLowerCase();
    if (s.includes('pending')) return 'pending';
    if (s.includes('confirm') || s.includes('process')) return 'progress';
    if (s.includes('ship')) return 'shipped';
    if (s.includes('deliver')) return 'delivered';
    if (s.includes('cancel')) return 'cancelled';
    if (s.includes('refund')) return 'refunded';
    return '';
  }

  trackHistory = (_: number, h: OrderStatusHistoryDto) => h.orderStatusHistoryId;
  starsTrack = (i: number) => i;
}
