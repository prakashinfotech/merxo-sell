import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-admin-login',
  templateUrl: './admin-login.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminLogin {
  form: FormGroup;
  loading      = signal(false);
  errorMessage = signal('');
  showPassword = false;

  heroData = {
    image: '/assets/images/auth/auth-admin.png',
    title: 'MerxoSell Control Center.',
    subtitle: 'Manage operations, security, and global analytics from one secure dashboard.'
  };

  constructor(
    private fb:     FormBuilder,
    private auth:   AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  get email()    { return this.form.get('email')!; }
  get password() { return this.form.get('password')!; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading.set(true);
    this.errorMessage.set('');

    this.auth.adminLogin(this.form.value).subscribe({
      next:  () => this.router.navigate(['/admin/dashboard']),
      error: (err) => {
        this.errorMessage.set(err.error?.error ?? 'Login failed. SuperAdmin credentials required.');
        this.loading.set(false);
      }
    });
  }
}
