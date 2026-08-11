import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UpdateOrderStatusDto, CancelOrderDto, OrderStatusHistoryDto,
} from '../order/order.service';

@Injectable({ providedIn: 'root' })
export class SellerOrdersService {
  private base = `${environment.apiUrl}/seller`;

  constructor(private http: HttpClient) {}

  list(search?: string, status?: string): Observable<unknown[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<unknown[]>(`${this.base}/orders`, { params });
  }

  detail(id: number): Observable<unknown> {
    return this.http.get(`${this.base}/orders/${id}`);
  }

  updateStatus(id: number, status: string, note?: string): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.base}/orders/${id}/status`,
      { status, note } satisfies UpdateOrderStatusDto);
  }

  cancel(id: number, reason: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/orders/${id}/cancel`,
      { cancellationReason: reason } satisfies CancelOrderDto);
  }

  history(id: number): Observable<OrderStatusHistoryDto[]> {
    return this.http.get<OrderStatusHistoryDto[]>(`${this.base}/orders/${id}/history`);
  }
}
