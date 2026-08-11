import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA, Pipe, PipeTransform } from '@angular/core';
import { Router } from '@angular/router';

@Pipe({ name: 'appCurrency', standalone: false })
class MockAppCurrencyPipe implements PipeTransform { transform(v: any) { return v; } }
import { BehaviorSubject, of } from 'rxjs';
import { Cart } from './cart';
import { CartService, CartItemDto, CartDto } from '../../core/cart/cart.service';
import { CurrencyService } from '../../core/currency/currency.service';
import { DialogService } from '../../core/dialog/dialog.service';
import { map } from 'rxjs/operators';

const mockItem: CartItemDto = {
  cartItemId: 1, productId: 10, productName: 'T-Shirt',
  unitPriceCAD: 25, quantity: 2, maxStock: 10
};

const cartSubject = new BehaviorSubject<CartDto | null>({
  cartId: 1, userId: 5, totalAmountCAD: 50, items: [mockItem]
});

describe('Cart component', () => {
  let component: Cart;
  let fixture: ComponentFixture<Cart>;
  let cartService: any;
  let dialogService: any;

  beforeEach(async () => {
    cartService = {
      cart$:       cartSubject.asObservable(),
      items$:      cartSubject.pipe(map(c => c?.items ?? [])),
      total$:      cartSubject.pipe(map(c => c?.totalAmountCAD ?? 0)),
      itemCount$:  cartSubject.pipe(map(c => c?.items.reduce((s, i) => s + i.quantity, 0) ?? 0)),
      fetchCart:   vi.fn(() => of({ cartId: 1, userId: 5, totalAmountCAD: 50, items: [mockItem] })),
      updateItem:  vi.fn(() => of({ ...cartSubject.value, items: [{ ...mockItem, quantity: 3 }] })),
      removeItem:  vi.fn(() => of({ ...cartSubject.value, items: [] })),
    };

    dialogService = {
      ask: vi.fn(() => of(true))
    };

    await TestBed.configureTestingModule({
      declarations: [Cart, MockAppCurrencyPipe],
      providers: [
        { provide: CartService,    useValue: cartService },
        { provide: CurrencyService, useValue: { selectedCurrency$: of({ code: 'CAD', symbol: '$', rate: 1 }) } },
        { provide: DialogService,  useValue: dialogService },
        { provide: Router,         useValue: { navigate: vi.fn() } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture   = TestBed.createComponent(Cart);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit() calls fetchCart()', () => {
    fixture.detectChanges();
    expect(cartService.fetchCart).toHaveBeenCalled();
  });

  it('updateQuantity() does nothing if new qty < 1', () => {
    component.updateQuantity(1, 1, -1);
    expect(cartService.updateItem).not.toHaveBeenCalled();
  });

  it('updateQuantity() calls cartService.updateItem when qty >= 1', () => {
    component.updateQuantity(1, 2, 1);
    expect(cartService.updateItem).toHaveBeenCalledWith(1, { quantity: 3 });
  });

  it('removeItem() asks for confirmation before deleting', () => {
    fixture.detectChanges();
    component.removeItem(1);
    expect(dialogService.ask).toHaveBeenCalled();
  });

  it('removeItem() calls cartService.removeItem after confirmation', () => {
    dialogService.ask.mockReturnValue(of(true));
    fixture.detectChanges();
    component.removeItem(1);
    expect(cartService.removeItem).toHaveBeenCalledWith(1);
  });
});
