import { Component, ChangeDetectionStrategy, signal, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

function passwordStrengthValidator(): ValidatorFn {
  return (ctrl: AbstractControl): ValidationErrors | null => {
    const val: string = ctrl.value ?? '';
    const errors: ValidationErrors = {};
    if (!/[A-Z]/.test(val))        errors['noUppercase'] = true;
    if (!/[a-z]/.test(val))        errors['noLowercase'] = true;
    if (!/[0-9]/.test(val))        errors['noDigit']     = true;
    if (!/[^A-Za-z0-9]/.test(val)) errors['noSpecial']   = true;
    return Object.keys(errors).length ? errors : null;
  };
}

function passwordsMatch(group: FormGroup): { mismatch: true } | null {
  const pw  = group.get('password')?.value;
  const cpw = group.get('confirmPassword')?.value;
  return pw === cpw ? null : { mismatch: true };
}

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Register implements OnInit {
  form:         FormGroup;
  loading      = signal(false);
  errorMessage = signal('');
  showPassword  = signal(false);
  passwordFocus = signal(false);

  heroData = {
    Buyer: {
      image: '/assets/images/auth/auth-buyer.png',
      title: 'Join our global community.',
      subtitle: 'Create an account to track orders, save favorites, and get exclusive deals.'
    },
    Seller: {
      image: '/assets/images/auth/seller.png',
      title: 'Start selling today.',
      subtitle: 'Reach millions of customers and grow your brand with our powerful platform.'
    }
  };

  constructor(
    private fb:      FormBuilder,
    private auth:    AuthService,
    private router:  Router,
    private route:   ActivatedRoute
  ) {
    this.form = this.fb.group({
      fullName:        ['', [Validators.required, Validators.maxLength(150)]],
      email:           ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      password:        ['', [Validators.required, Validators.minLength(8), passwordStrengthValidator()]],
      confirmPassword: ['', Validators.required],
      role:            ['Buyer', Validators.required],
      storeName:       ['', [Validators.maxLength(200)]]
    }, { validators: passwordsMatch });

    this.form.get('role')?.valueChanges.subscribe(role => {
      const storeNameCtrl = this.form.get('storeName');
      if (role === 'Seller') {
        storeNameCtrl?.setValidators([Validators.required, Validators.maxLength(200)]);
      } else {
        storeNameCtrl?.clearValidators();
        storeNameCtrl?.setValidators([Validators.maxLength(200)]);
      }
      storeNameCtrl?.updateValueAndValidity();
    });
  }
  
  ngOnInit(): void {
    const roleParam = this.route.snapshot.queryParamMap.get('role');
    if (roleParam === 'Seller' || roleParam === 'Buyer') {
      this.role.setValue(roleParam);
    }
  }

  get currentHero() {
    return this.heroData[this.role.value as 'Buyer' | 'Seller'];
  }

  get fullName()        { return this.form.get('fullName')!; }
  get email()           { return this.form.get('email')!; }
  get password()        { return this.form.get('password')!; }
  get confirmPassword() { return this.form.get('confirmPassword')!; }
  get role()            { return this.form.get('role')!; }
  get storeName()       { return this.form.get('storeName')!; }

  get pwVal(): string { return this.password.value ?? ''; }

  get hasMinLength(): boolean { return this.pwVal.length >= 8; }
  get hasUppercase(): boolean { return /[A-Z]/.test(this.pwVal); }
  get hasLowercase(): boolean { return /[a-z]/.test(this.pwVal); }
  get hasDigit():     boolean { return /[0-9]/.test(this.pwVal); }
  get hasSpecial():   boolean { return /[^A-Za-z0-9]/.test(this.pwVal); }

  get strengthScore(): number {
    return [this.hasMinLength, this.hasUppercase, this.hasLowercase, this.hasDigit, this.hasSpecial]
      .filter(Boolean).length;
  }

  get strengthLabel(): 'weak' | 'fair' | 'strong' {
    if (this.strengthScore <= 2) return 'weak';
    if (this.strengthScore <= 3) return 'fair';
    return 'strong';
  }

  get showStrength(): boolean {
    return this.passwordFocus();
  }

  togglePassword(): void { this.showPassword.update(v => !v); }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading.set(true);
    this.errorMessage.set('');

    this.auth.register(this.form.value).subscribe({
      next:  () => {
        if (this.role.value === 'Seller') {
          this.router.navigate(['/seller']);
          return;
        }
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        const target = returnUrl ?? '/';
        this.router.navigateByUrl(target);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.error ?? 'Registration failed. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
