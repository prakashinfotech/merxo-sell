import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';

/**
 * Picks the Bearer token that matches the URL being called:
 *   /api/admin/*   → admin token
 *   /api/seller/*  → seller token
 *   shared URLs → token for the currently open portal tab
 *
 * Auth endpoints (login/register) are passed through untouched.
 */
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService, private router: Router) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (req.url.includes('/auth/')) return next.handle(req);

    const token = this.auth.getTokenForUrl(req.url) ?? this.getTokenForCurrentPortal();
    if (token) {
      req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }
    return next.handle(req);
  }

  private getTokenForCurrentPortal(): string | null {
    if (this.router.url.startsWith('/admin')) return this.auth.getTokenFor('admin');
    if (this.router.url.startsWith('/seller')) return this.auth.getTokenFor('seller');
    return this.auth.getTokenFor('buyer');
  }
}
