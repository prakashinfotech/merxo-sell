import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProfileService, UserProfileDto } from '../../../core/profile/profile.service';
import { CurrencyService } from '../../../core/currency/currency.service';
import { AuthService } from '../../../core/auth/auth.service';
import { DialogService } from '../../../core/dialog/dialog.service';

@Component({
  selector: 'app-seller-profile',
  templateUrl: './seller-profile.html',
  styleUrls: ['./seller-profile.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SellerProfile implements OnInit {
  profile: UserProfileDto | null = null;
  loading = true;
  activeTab: 'info' | 'store' | 'security' = 'info';
  
  profileForm: FormGroup;
  passwordForm: FormGroup;
  storeForm: FormGroup;
  
  submitting = false;
  store: any = null;

  constructor(
    private profileService: ProfileService,
    public currencyService: CurrencyService,
    public authService: AuthService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private dialogService: DialogService
  ) {
    this.profileForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      phone: [''],
      preferredCurrency: ['CAD', Validators.required]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.storeForm = this.fb.group({
      storeName: ['', [Validators.required, Validators.maxLength(200)]],
      storeDescription: ['', [Validators.maxLength(1000)]],
      contactEmail: ['', [Validators.email]],
      phone: ['']
    });
  }

  private passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loading = true;
    this.profileService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.profileForm.patchValue({
          fullName: profile.fullName,
          phone: profile.phone,
          preferredCurrency: profile.preferredCurrency
        });
        
        this.profileService.getSellerStore().subscribe({
          next: (store) => {
            this.store = store;
            this.storeForm.patchValue({
              storeName: store.storeName,
              storeDescription: store.storeDescription,
              contactEmail: store.contactEmail,
              phone: store.phone
            });
            this.loading = false;
            this.cdr.markForCheck();
          },
          error: () => {
            this.loading = false;
            this.cdr.markForCheck();
          }
        });
      },
      error: () => {
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  updateProfile(): void {
    if (this.profileForm.invalid) return;
    this.submitting = true;
    this.profileService.updateProfile(this.profileForm.value).subscribe({
      next: (updated) => {
        this.profile = updated;
        this.submitting = false;
        this.dialogService.notify('Success', 'Profile updated successfully!');
        this.cdr.markForCheck();
      },
      error: () => { this.submitting = false; this.dialogService.notify('Error', 'Update failed.'); this.cdr.markForCheck(); }
    });
  }

  updatePassword(): void {
    if (this.passwordForm.invalid) return;
    this.submitting = true;
    this.profileService.changePassword(this.passwordForm.value).subscribe({
      next: () => {
        this.submitting = false;
        this.passwordForm.reset();
        this.dialogService.notify('Success', 'Password changed successfully!');
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.submitting = false;
        this.dialogService.notify('Error', err?.error?.error || 'Password change failed.');
        this.cdr.markForCheck();
      }
    });
  }

  updateStore(): void {
    if (this.storeForm.invalid) return;
    this.submitting = true;
    this.profileService.updateSellerStore(this.storeForm.value).subscribe({
      next: () => {
        this.submitting = false;
        this.dialogService.notify('Success', 'Store settings updated successfully!');
        this.cdr.markForCheck();
      },
      error: () => { this.submitting = false; this.dialogService.notify('Error', 'Update failed.'); this.cdr.markForCheck(); }
    });
  }
}
