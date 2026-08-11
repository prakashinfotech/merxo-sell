import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CartService, CartDto } from './cart.service';

const mockCart: CartDto = {
  cartId: 1,
  userId: 10,
  totalAmountCAD: 75,
  items: [
    { cartItemId: 1, productId: 100, productName: 'Shirt', unitPriceCAD: 25, quantity: 3, maxStock: 10 }
  ]
};

describe('CartService', () => {
  let service: CartService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CartService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(CartService);
    http    = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('fetchCart() GETs /cart and updates cart$ subject', () => {
    let emitted: CartDto | null = null;
    service.cart$.subscribe(c => emitted = c);

    service.fetchCart().subscribe();

    const req = http.expectOne('http://localhost:5000/api/cart');
    expect(req.request.method).toBe('GET');
    req.flush(mockCart);

    expect(emitted).toEqual(mockCart);
  });

  it('items$ reflects items from fetched cart', () => {
    const items: any[] = [];
    service.items$.subscribe(i => items.push(...i));

    service.fetchCart().subscribe();
    http.expectOne('http://localhost:5000/api/cart').flush(mockCart);

    expect(items.length).toBeGreaterThan(0);
    expect(items[0].productName).toBe('Shirt');
  });

  it('total$ reflects totalAmountCAD from fetched cart', () => {
    let total = 0;
    service.total$.subscribe(t => total = t);

    service.fetchCart().subscribe();
    http.expectOne('http://localhost:5000/api/cart').flush(mockCart);

    expect(total).toBe(75);
  });

  it('addItem() POSTs to /cart/items', () => {
    service.addItem({ productId: 100, quantity: 2 }).subscribe();

    const req = http.expectOne('http://localhost:5000/api/cart/items');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.productId).toBe(100);
    req.flush(mockCart);
  });

  it('updateItem() PUTs to /cart/items/:id', () => {
    service.updateItem(5, { quantity: 3 }).subscribe();

    const req = http.expectOne('http://localhost:5000/api/cart/items/5');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body.quantity).toBe(3);
    req.flush(mockCart);
  });

  it('removeItem() DELETEs /cart/items/:id', () => {
    service.removeItem(1).subscribe();

    const req = http.expectOne('http://localhost:5000/api/cart/items/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(mockCart);
  });

  it('itemCount$ returns 0 when cart is empty', () => {
    let count = -1;
    service.itemCount$.subscribe(n => count = n);
    expect(count).toBe(0);
  });
});
