import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';

@Component({ template: '', standalone: true })
class StubComponent {}

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', component: StubComponent }]),
      ],
    });
    service = TestBed.inject(AuthService);
    http    = TestBed.inject(HttpTestingController);
    sessionStorage.clear();
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  // ── register ────────────────────────────────────────────────────────────────

  it('register() POSTs to /auth/register and stores buyer token', () => {
    const mockResponse = {
      success: true,
      data: {
        accessToken: 'fake-token',
        expiresIn: 3600,
        userId: 1,
        email: 'test@test.com',
        role: 'Buyer',
        preferredCurrency: 'CAD',
        fullName: 'Test User',
      }
    };

    service.register({ fullName: 'Test', email: 'test@test.com', password: 'Abc@1234!', confirmPassword: 'Abc@1234!' })
      .subscribe(res => {
        expect(res.accessToken).toBe('fake-token');
      });

    const req = http.expectOne('http://localhost:5000/api/auth/register');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  // ── login ────────────────────────────────────────────────────────────────────

  it('login() POSTs to /auth/login', () => {
    const mockResponse = {
      success: true,
      data: {
        accessToken: 'buyer-token', expiresIn: 3600, userId: 2,
        email: 'buyer@test.com', role: 'Buyer', preferredCurrency: 'CAD', fullName: 'Buyer'
      }
    };

    service.login({ email: 'buyer@test.com', password: 'Buyer@1234!' }).subscribe();

    const req = http.expectOne('http://localhost:5000/api/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'buyer@test.com', password: 'Buyer@1234!' });
    req.flush(mockResponse);
  });

  it('adminLogin() POSTs to /auth/admin/login', () => {
    const mockResponse = {
      success: true,
      data: {
        accessToken: 'admin-token', expiresIn: 3600, userId: 99,
        email: 'admin@test.com', role: 'SuperAdmin', preferredCurrency: 'CAD', fullName: 'Admin'
      }
    };

    service.adminLogin({ email: 'admin@test.com', password: 'Admin@123' }).subscribe();

    const req = http.expectOne('http://localhost:5000/api/auth/admin/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('sellerLogin() POSTs to /auth/seller/login', () => {
    const mockResponse = {
      success: true,
      data: {
        accessToken: 'seller-token', expiresIn: 3600, userId: 50,
        email: 'seller@test.com', role: 'Seller', preferredCurrency: 'CAD', fullName: 'Seller',
        sellerId: 5
      }
    };

    service.sellerLogin({ email: 'seller@test.com', password: 'Seller@123' }).subscribe();

    const req = http.expectOne('http://localhost:5000/api/auth/seller/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  // ── token / logout helpers ─────────────────────────────────────────────────

  it('getToken() returns null when no session exists', () => {
    expect(service.getToken()).toBeNull();
  });

  it('isLoggedIn() returns false when no token stored', () => {
    expect(service.isLoggedIn()).toBe(false);
  });

  it('logout() clears all session scopes', () => {
    sessionStorage.setItem('tc_buyer_token',  'tok1');
    sessionStorage.setItem('tc_seller_token', 'tok2');
    sessionStorage.setItem('tc_admin_token',  'tok3');

    service.logout();

    expect(sessionStorage.getItem('tc_buyer_token')).toBeNull();
    expect(sessionStorage.getItem('tc_seller_token')).toBeNull();
    expect(sessionStorage.getItem('tc_admin_token')).toBeNull();
  });

  it('logoutScope(buyer) only removes buyer token', () => {
    sessionStorage.setItem('tc_buyer_token',  'b-tok');
    sessionStorage.setItem('tc_seller_token', 's-tok');

    service.logoutScope('buyer');

    expect(sessionStorage.getItem('tc_buyer_token')).toBeNull();
    expect(sessionStorage.getItem('tc_seller_token')).toBe('s-tok');
  });

  it('getTokenForUrl() returns admin token for /admin/ URLs', () => {
    sessionStorage.setItem('tc_admin_token', 'admin-tok');

    const token = service.getTokenForUrl('/api/admin/users');

    expect(token).toBe('admin-tok');
  });

  it('getTokenForUrl() returns seller token for /seller/ URLs', () => {
    sessionStorage.setItem('tc_seller_token', 'seller-tok');

    const token = service.getTokenForUrl('/api/seller/products');

    expect(token).toBe('seller-tok');
  });
});
