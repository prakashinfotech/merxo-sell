import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA, Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'appCurrency', standalone: false })
class MockAppCurrencyPipe implements PipeTransform { transform(v: any) { return v; } }
import { Router } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { Checkout } from './checkout';
import { CartService } from '../../core/cart/cart.service';
import { OrderService } from '../../core/order/order.service';
import { CouponService } from '../../core/coupon/coupon.service';
import { CurrencyService } from '../../core/currency/currency.service';
import { ProfileService } from '../../core/profile/profile.service';

const mockProfile = {
  userId: 1,
  fullName: 'Test Buyer',
  email: 'buyer@test.com',
  preferredCurrency: 'CAD',
  country: 'Canada',
  createdAt: new Date().toISOString(),
  addresses: [
    { addressId: 10, fullName: 'Test', addressLine1: '1 Main', city: 'Toronto', state: 'ON',
      postalCode: 'M1A', country: 'Canada', isDefault: true }
  ],
  paymentMethods: []
};

describe('Checkout component', () => {
  let component: Checkout;
  let fixture: ComponentFixture<Checkout>;
  let profileService: any;
  let cartService: any;
  let couponService: any;
  let orderService: any;

  beforeEach(async () => {
    profileService = {
      getProfile:   vi.fn(() => of(mockProfile)),
      addAddress:   vi.fn(() => of({ ...mockProfile.addresses[0], addressId: 99 })),
    };

    cartService = {
      cart$:      of(null),
      items$:     of([]),
      total$:     of(0),
      itemCount$: of(0),
      fetchCart:  vi.fn(() => of({ cartId: 1, userId: 1, totalAmountCAD: 100, items: [] })),
    };

    couponService = {
      getAvailable: vi.fn(() => of([])),
      apply:        vi.fn(),
    };

    orderService = {
      createOrder: vi.fn(() => of({ orderId: 1, status: 'Pending', items: [] })),
    };

    await TestBed.configureTestingModule({
      declarations: [Checkout, MockAppCurrencyPipe],
      providers: [
        { provide: ProfileService,  useValue: profileService },
        { provide: CartService,     useValue: cartService },
        { provide: OrderService,    useValue: orderService },
        { provide: CouponService,   useValue: couponService },
        { provide: CurrencyService, useValue: { selectedCurrency$: of({ code: 'CAD', symbol: '$', rate: 1 }) } },
        { provide: Router,          useValue: { navigate: vi.fn() } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture   = TestBed.createComponent(Checkout);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit() loads profile and pre-selects default address', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.addresses).toHaveLength(1);
    expect(component.selectedAddressId).toBe(10);
  });

  it('loading is false after OnInit completes', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.loading).toBe(false);
  });

  it('saveNewAddress() sets error when required fields are empty', () => {
    component.newAddress = {
      fullName: '', addressLine1: '', city: '', state: '', postalCode: '', country: ''
    } as any;

    component.saveNewAddress();

    expect(component.newAddressError).toBeTruthy();
    expect(profileService.addAddress).not.toHaveBeenCalled();
  });

  it('toggleNewAddressForm() shows and hides the address form', () => {
    fixture.detectChanges();

    expect(component.showNewAddressForm).toBe(false);
    component.toggleNewAddressForm();
    expect(component.showNewAddressForm).toBe(true);
    component.toggleNewAddressForm();
    expect(component.showNewAddressForm).toBe(false);
  });

  it('appliedCoupon is null initially', () => {
    expect(component.appliedCoupon).toBeNull();
  });
});
