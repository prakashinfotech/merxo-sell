import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';

export interface LoginRequest    { email: string; password: string; }
export interface RegisterRequest { fullName: string; email: string; password: string; confirmPassword: string; role?: string; storeName?: string; }
export interface AuthResponse {
  accessToken: string; expiresIn: number;
  userId: number; email: string; role: string; preferredCurrency: string;
  fullName?: string; sellerId?: number;
}

interface JwtPayload { sub: string; email: string; role: string; preferredCurrency: string; sellerId?: string; exp: number; }

/**
 * Per-tab session slots. sessionStorage is intentionally used so Admin,
 * Seller, and Buyer logins opened in different browser tabs do not share or
 * overwrite each other's identity.
 */
export type AuthScope = 'buyer' | 'seller' | 'admin';

const TOKEN_KEYS: Record<AuthScope, string> = {
  buyer:  'tc_buyer_token',
  seller: 'tc_seller_token',
  admin:  'tc_admin_token',
};

const ACTIVE_KEY = 'tc_active_scope';
const LEGACY_TOKEN_KEY = 'access_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient, private router: Router) {
    this.migrateLegacyToken();
  }

  // ── Login flows ────────────────────────────────────────────────────────────
  register(dto: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/register`, dto).pipe(
      map(res => res.data),
      tap(res => this.storeForRole(res.accessToken, res.role)),
    );
  }

  /** Buyer (regular user) login — only stores a buyer-scope token. */
  login(dto: LoginRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/login`, dto).pipe(
      map(res => res.data),
      tap(res => this.storeToken(res.accessToken, 'buyer')),
    );
  }

  /** SuperAdmin login — stores token in the admin slot only. */
  adminLogin(dto: LoginRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/admin/login`, dto).pipe(
      map(res => res.data),
      tap(res => this.storeToken(res.accessToken, 'admin')),
    );
  }

  /** Seller login — stores token in the seller slot only. */
  sellerLogin(dto: LoginRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/seller/login`, dto).pipe(
      map(res => res.data),
      tap(res => this.storeToken(res.accessToken, 'seller')),
    );
  }

  // ── Token access ───────────────────────────────────────────────────────────
  /** Returns the token for an explicit scope. */
  getTokenFor(scope: AuthScope): string | null {
    return sessionStorage.getItem(TOKEN_KEYS[scope]);
  }

  /**
   * Returns the token for URLs that clearly belong to one auth scope.
   * Shared URLs return null so the interceptor can use the current portal route.
   */
  getTokenForUrl(url: string): string | null {
    if (url.includes('/admin/') || url.includes('/reviews/admin') || url.includes('/auth/admin')) {
      return this.getTokenFor('admin');
    }
    if (url.includes('/seller/') || url.includes('/reviews/seller') || url.includes('/auth/seller')) {
      return this.getTokenFor('seller');
    }
    return null;
  }

  /**
   * Backwards-compatible single-token getter. Resolves in priority order:
   *   active scope (set by latest login) → buyer → seller → admin.
   * Most call sites should switch to {@link getTokenForUrl}.
   */
  getToken(): string | null {
    const active = (sessionStorage.getItem(ACTIVE_KEY) as AuthScope | null);
    const ordered: AuthScope[] = active
      ? [active, ...(['buyer', 'seller', 'admin'] as AuthScope[]).filter(s => s !== active)]
      : ['buyer', 'seller', 'admin'];

    for (const scope of ordered) {
      const t = this.getTokenFor(scope);
      if (t) return t;
    }
    return null;
  }

  // ── Logout helpers ─────────────────────────────────────────────────────────
  /** Clears every session and routes to the buyer login screen. */
  logout(): void {
    this.logoutScope('buyer');
    this.logoutScope('seller');
    this.logoutScope('admin');
    sessionStorage.removeItem(ACTIVE_KEY);
    this.router.navigate(['/auth/login']);
  }

  /** Clears just one scope; the others remain logged in. */
  logoutScope(scope: AuthScope): void {
    sessionStorage.removeItem(TOKEN_KEYS[scope]);
    if (sessionStorage.getItem(ACTIVE_KEY) === scope) {
      sessionStorage.removeItem(ACTIVE_KEY);
    }
  }

  // ── Identity / role helpers ────────────────────────────────────────────────
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    const payload = this.decodeToken(token);
    return payload ? payload.exp * 1000 > Date.now() : false;
  }

  isLoggedInAs(scope: AuthScope): boolean {
    return this.hasActiveScope(scope);
  }

  /** True if the buyer slot has a valid (non-expired) token. */
  hasActiveScope(scope: AuthScope): boolean {
    const t = this.getTokenFor(scope);
    if (!t) return false;
    const payload = this.decodeToken(t);
    return !!payload && payload.exp * 1000 > Date.now();
  }

  getRole(): string | null {
    return this.decodePayload()?.role ?? null;
  }

  getEmail(): string | null {
    return this.decodePayload()?.email ?? null;
  }

  getRoleFor(scope: AuthScope): string | null {
    const token = this.getTokenFor(scope);
    return token ? this.decodeToken(token)?.role ?? null : null;
  }

  getPreferredCurrency(): string {
    return this.decodePayload()?.preferredCurrency ?? 'CAD';
  }

  getSellerId(): number | null {
    const payload = this.decodeToken(this.getTokenFor('seller') ?? '');
    return payload?.sellerId ? parseInt(payload.sellerId, 10) : null;
  }

  isAdmin(): boolean {
    return this.isSuperAdmin();
  }

  isSuperAdmin(): boolean {
    return this.hasRole('admin', 'superadmin');
  }

  isSeller(): boolean {
    return this.hasRole('seller', 'seller');
  }

  isBuyer(): boolean {
    return this.hasRole('buyer', 'buyer');
  }

  // ── Internals ──────────────────────────────────────────────────────────────
  private storeForRole(token: string, role: string): void {
    const lower = role.toLowerCase();
    if (lower === 'superadmin') this.storeToken(token, 'admin');
    else if (lower === 'seller')                     this.storeToken(token, 'seller');
    else                                             this.storeToken(token, 'buyer');
  }

  private storeToken(token: string, scope: AuthScope): void {
    sessionStorage.setItem(TOKEN_KEYS[scope], token);
    sessionStorage.setItem(ACTIVE_KEY, scope);
  }

  /** Move a token saved under the legacy single-key into the right scope slot. */
  private migrateLegacyToken(): void {
    const legacy = sessionStorage.getItem(LEGACY_TOKEN_KEY);
    if (!legacy) return;

    const payload = this.decodeToken(legacy);
    if (!payload) {
      sessionStorage.removeItem(LEGACY_TOKEN_KEY);
      return;
    }

    const role = payload.role?.toLowerCase();
    const scope: AuthScope =
      role === 'superadmin' ? 'admin'
      : role === 'seller'                       ? 'seller'
      :                                           'buyer';

    if (!this.getTokenFor(scope)) this.storeToken(legacy, scope);
    sessionStorage.removeItem(LEGACY_TOKEN_KEY);
  }

  private hasRole(scope: AuthScope, role: string): boolean {
    const token = this.getTokenFor(scope);
    if (!token) return false;

    const payload = this.decodeToken(token);
    if (!payload || payload.exp * 1000 <= Date.now()) return false;

    return payload.role?.toLowerCase() === role;
  }

  private decodePayload(): JwtPayload | null {
    const token = this.getToken();
    return token ? this.decodeToken(token) : null;
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      const payload = token.split('.')[1];
      if (!payload) return null;

      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=');

      return JSON.parse(atob(padded));
    } catch (err) {
      console.error('JWT Decode Error:', err);
      return null;
    }
  }
}
