import { Component, ChangeDetectionStrategy, signal, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login implements OnInit {
  form: FormGroup;
  loading      = signal(false);
  errorMessage = signal('');
  role         = signal<'buyer' | 'seller' | 'admin'>('buyer');
  showPassword = false;

  heroData = {
    buyer: {
      image: '/assets/images/auth/auth-buyer.png',
      title: 'Your global marketplace awaits.',
      subtitle: 'Millions of products, unbeatable prices, and lightning-fast delivery.'
    },
    seller: {
      image: '/assets/images/auth/seller.png',
      title: 'Grow your business with MerxoSell.',
      subtitle: 'Powerful inventory tools, secure payouts, and millions of active buyers.'
    },
    admin: {
      image: '/assets/images/auth/auth-admin.png',
      title: 'Platform Control Center',
      subtitle: 'Advanced analytics, seller verification, and marketplace governance.'
    }
  };

  private returnUrl: string | null = null;

  constructor(
    private fb:      FormBuilder,
    private auth:    AuthService,
    private router:  Router,
    private route:   ActivatedRoute
  ) {
    this.form = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.route.url.subscribe(url => {
      const isSeller = url.some(segment => segment.path === 'seller');
      const isAdmin  = url.some(segment => segment.path === 'admin');
      
      if (isAdmin) this.role.set('admin');
      else if (isSeller) this.role.set('seller');
      else this.role.set('buyer');
    });
    this.route.queryParams.subscribe(qp => {
      this.returnUrl = qp['returnUrl'] || null;
    });
  }

  get currentHero() {
    return this.heroData[this.role()];
  }

  get email()    { return this.form.get('email')!; }
  get password() { return this.form.get('password')!; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading.set(true);
    this.errorMessage.set('');

    const role = this.role();
    let loginObs;

    if (role === 'admin') loginObs = this.auth.adminLogin(this.form.value);
    else if (role === 'seller') loginObs = this.auth.sellerLogin(this.form.value);
    else loginObs = this.auth.login(this.form.value);

    loginObs.subscribe({
      next:  () => this.routeAfterLogin(),
      error: (err) => {
        this.errorMessage.set(err.error?.error ?? 'Login failed. Please try again.');
        this.loading.set(false);
      },
    });
  }

  private routeAfterLogin(): void {
    const role = this.role();
    if (role === 'admin') {
      this.router.navigate(['/admin/dashboard']);
      return;
    }
    if (role === 'seller') {
      this.router.navigate(['/seller/dashboard']);
      return;
    }
    const target = this.returnUrl ?? '/';
    this.router.navigateByUrl(target);
  }
}
