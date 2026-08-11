import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CartItemDto {
  cartItemId: number;
  productId: number;
  variantId?: number;
  productName: string;
  primaryImageUrl?: string;
  variantInfo?: string;
  unitPriceCAD: number;
  quantity: number;
  maxStock: number;
}

export interface CartDto {
  cartId: number;
  userId: number;
  totalAmountCAD: number;
  items: CartItemDto[];
}

export interface AddToCartDto {
  productId: number;
  variantId?: number;
  quantity: number;
}

export interface UpdateCartItemDto {
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private apiUrl = `${environment.apiUrl}/cart`;
  
  private readonly _cart$ = new BehaviorSubject<CartDto | null>(null);
  readonly cart$: Observable<CartDto | null> = this._cart$.asObservable();

  private readonly _deliveryMode$ = new BehaviorSubject<'standard' | 'express' | 'pickup'>('standard');
  readonly deliveryMode$ = this._deliveryMode$.asObservable();

  get currentDeliveryMode(): 'standard' | 'express' | 'pickup' {
    return this._deliveryMode$.value;
  }

  get shippingAmountCAD(): number {
    return this._deliveryMode$.value === 'express' ? 9.99 : 0;
  }

  setDeliveryMode(mode: 'standard' | 'express' | 'pickup'): void {
    this._deliveryMode$.next(mode);
  }

  readonly items$: Observable<CartItemDto[]> = this._cart$.pipe(
    map(cart => cart?.items ?? [])
  );

  readonly itemCount$: Observable<number> = this._cart$.pipe(
    map(cart => cart?.items.reduce((sum, i) => sum + i.quantity, 0) ?? 0)
  );

  readonly total$: Observable<number> = this._cart$.pipe(
    map(cart => cart?.totalAmountCAD ?? 0)
  );

  constructor(private http: HttpClient) {}

  fetchCart(): Observable<CartDto> {
    return this.http.get<CartDto>(this.apiUrl).pipe(
      tap(cart => this._cart$.next(cart))
    );
  }

  addItem(dto: AddToCartDto): Observable<CartDto> {
    return this.http.post<CartDto>(`${this.apiUrl}/items`, dto).pipe(
      tap(cart => this._cart$.next(cart))
    );
  }

  updateItem(itemId: number, dto: UpdateCartItemDto): Observable<CartDto> {
    return this.http.put<CartDto>(`${this.apiUrl}/items/${itemId}`, dto).pipe(
      tap(cart => this._cart$.next(cart))
    );
  }

  removeItem(itemId: number): Observable<CartDto> {
    return this.http.delete<CartDto>(`${this.apiUrl}/items/${itemId}`).pipe(
      tap(cart => this._cart$.next(cart))
    );
  }

  clearCart(): Observable<void> {
    return this.http.delete<void>(this.apiUrl).pipe(
      tap(() => this._cart$.next(null))
    );
  }

  /** Clears the in-memory cart state without calling the API (used on logout). */
  resetLocal(): void {
    this._cart$.next(null);
  }
}
