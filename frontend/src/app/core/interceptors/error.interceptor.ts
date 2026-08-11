import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Centralised HTTP error handling. On 401 we only clear the scope that owns
 * the failing request — admins stay logged in if the buyer session expires
 * (and vice versa) — and we never redirect from auth-endpoint failures so
 * the login form can show the validation error inline.
 */
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router, private auth: AuthService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      catchError((err: HttpErrorResponse) => {
        const isAuthEndpoint = req.url.includes('/auth/');

        if (err.status === 401 && !isAuthEndpoint) {
          const currentUrl = this.router.url;
          if (req.url.includes('/admin/') || currentUrl.startsWith('/admin')) {
            this.auth.logoutScope('admin');
            this.router.navigate(['/auth/admin/login']);
          } else if (req.url.includes('/seller/') || currentUrl.startsWith('/seller')) {
            this.auth.logoutScope('seller');
            this.router.navigate(['/auth/seller/login']);
          } else {
            this.auth.logoutScope('buyer');
            this.router.navigate(['/auth/login']);
          }
        }

        if (err.status >= 500) {
          console.error('Server error:', err.message);
          this.router.navigate(['/500']);
        }

        return throwError(() => err);
      }),
    );
  }
}
