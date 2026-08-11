import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CouponService, CouponValidationResultDto } from './coupon.service';

describe('CouponService', () => {
  let service: CouponService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CouponService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(CouponService);
    http    = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('getAvailable() GETs /coupons/available', () => {
    service.getAvailable().subscribe();

    const req = http.expectOne('http://localhost:5000/api/coupons/available');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('apply() POSTs to /coupons/apply with couponCode and orderAmount', () => {
    service.apply('SAVE10', 100).subscribe();

    const req = http.expectOne('http://localhost:5000/api/coupons/apply');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.couponCode).toBe('SAVE10');
    expect(req.request.body.orderAmount).toBe(100);
    req.flush({ isValid: true, discountAmount: 10, finalAmount: 90, orderAmount: 100 });
  });

  it('getAllAdmin() GETs /admin/coupons', () => {
    service.getAllAdmin().subscribe();

    const req = http.expectOne('http://localhost:5000/api/admin/coupons');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('create() POSTs to /admin/coupons', () => {
    service.create({ couponCode: 'NEW', discountValue: 10, discountType: 'FixedAmount' } as any).subscribe();

    const req = http.expectOne('http://localhost:5000/api/admin/coupons');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('setActive() PATCHes /admin/coupons/:id/active', () => {
    service.setActive(5, false).subscribe();

    const req = http.expectOne('http://localhost:5000/api/admin/coupons/5/active');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body.isActive).toBe(false);
    req.flush({});
  });

  it('delete() DELETEs /admin/coupons/:id', () => {
    service.delete(3).subscribe();

    const req = http.expectOne('http://localhost:5000/api/admin/coupons/3');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
