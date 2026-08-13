import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OrderStatusHistoryDto } from '../order/order.service';

export type OrderStatus =
  | 'Pending' | 'Confirmed' | 'Processing'
  | 'Shipped' | 'Delivered' | 'Cancelled' | 'Refunded';

export interface AdminOrderListDto {
  orderId: number;
  status: OrderStatus;
  totalAmountCAD: number;
  currencyCode: string;
  displayTotal: number;
  itemCount: number;
  userId: number;
  buyerName: string;
  buyerEmail: string;
  cancellationReason?: string;
  createdAt: string;
  updatedAt?: string;
  paymentMethod?: string;
  paymentTransactionId?: string;
}

export interface AdminOrderItemDto {
  orderItemId: number;
  productId: number;
  variantId?: number;
  productName: string;
  primaryImageUrl?: string;
  variantInfo?: string;
  quantity: number;
  unitPriceCAD: number;
}

export interface AdminOrderShippingAddressDto {
  fullName: string;
  phone: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode: string;
  country: string;
}

export interface AdminOrderDetailDto {
  orderId: number;
  status: OrderStatus;
  totalAmountCAD: number;
  shippingAmount: number;
  discountAmount: number;
  displayTotal: number;
  currencyCode: string;
  notes?: string;
  cancellationReason?: string;
  createdAt: string;
  updatedAt?: string;
  userId: number;
  buyerName: string;
  buyerEmail: string;
  buyerPhone?: string;
  shippingAddress?: AdminOrderShippingAddressDto;
  items: AdminOrderItemDto[];
  paymentMethod?: string;
  paymentTransactionId?: string;
}

@Injectable({ providedIn: 'root' })
export class AdminOrdersService {
  private readonly base = `${environment.apiUrl}/admin/orders`;
  private readonly ordersBase = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, status?: string): Observable<AdminOrderListDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<AdminOrderListDto[]>(this.base, { params });
  }

  getById(id: number): Observable<AdminOrderDetailDto> {
    return this.http.get<AdminOrderDetailDto>(`${this.base}/${id}`);
  }

  updateStatus(id: number, status: OrderStatus, note?: string): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.base}/${id}/status`, { status, note });
  }

  cancel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  /** Status timeline — shared global endpoint (any authenticated admin). */
  history(id: number): Observable<OrderStatusHistoryDto[]> {
    return this.http.get<OrderStatusHistoryDto[]>(`${this.ordersBase}/${id}/history`);
  }
}
