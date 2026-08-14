import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrderItemDto {
  orderItemId: number;
  productId: number;
  variantId?: number;
  productName: string;
  primaryImageUrl?: string;
  variantInfo?: string;
  quantity: number;
  unitPriceCAD: number;
}

export interface OrderDto {
  orderId: number;
  status: string;
  totalAmountCAD: number;
  shippingAmount: number;
  discountAmount: number;
  couponCode?: string;
  displayTotal: number;
  currencyCode: string;
  createdAt: string;
  cancellationReason?: string;
  items: OrderItemDto[];
  paymentMethod?: string;
  paymentTransactionId?: string;
}

export interface CreateOrderDto {
  addressId: number;
  currencyCode: string;
  displayTotal: number;
  shippingAmount: number;
  discountAmount: number;
  couponCode?: string;
  paymentMethod?: string;
  paymentTransactionId?: string;
  items?: CreateOrderItemDto[];
}

export interface CreateOrderItemDto {
  productId: number;
  variantId?: number;
  quantity: number;
}

export interface UpdateOrderStatusDto {
  status: string;
  note?: string;
}

export interface CancelOrderDto {
  cancellationReason: string;
}

export interface OrderStatusHistoryDto {
  orderStatusHistoryId: number;
  orderId: number;
  fromStatus: string;
  toStatus: string;
  note?: string;
  changedBy?: number;
  changedByName?: string;
  changedByRole?: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  // ── Buyer flows
  createOrder(dto: CreateOrderDto): Observable<OrderDto> {
    return this.http.post<OrderDto>(this.apiUrl, dto);
  }
  getUserOrders(): Observable<OrderDto[]> {
    return this.http.get<OrderDto[]>(this.apiUrl);
  }
  getOrder(id: number): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${this.apiUrl}/${id}`);
  }
  getOrderHistory(id: number): Observable<OrderStatusHistoryDto[]> {
    return this.http.get<OrderStatusHistoryDto[]>(`${this.apiUrl}/${id}/history`);
  }
  cancelOrder(id: number, reason: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/cancel`, {
      cancellationReason: reason,
    } satisfies CancelOrderDto);
  }

  // ── Admin flows
  getAllOrders(): Observable<OrderDto[]> {
    return this.http.get<OrderDto[]>(`${this.apiUrl}/all`);
  }
  updateOrderStatus(id: number, status: string, note?: string): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/${id}/status`, {
      status, note,
    } satisfies UpdateOrderStatusDto);
  }
}
