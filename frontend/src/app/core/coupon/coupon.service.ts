import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type DiscountType = 'FixedAmount' | 'Percentage';

export interface CouponDto {
  couponId: number;
  couponCode: string;
  title: string;
  description?: string;
  discountType: DiscountType;
  discountValue: number;
  minimumPurchaseAmount?: number;
  maximumDiscountAmount?: number;
  usageLimit?: number;
  usedCount: number;
  startDate: string;
  expiryDate?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  currencyCode: string;
}

export type CouponSummaryDto = Omit<CouponDto, 'isActive' | 'createdAt' | 'updatedAt'>;

export interface CouponValidationResultDto {
  isValid: boolean;
  message?: string;
  couponCode?: string;
  orderAmount: number;
  discountAmount: number;
  finalAmount: number;
}

export interface CouponUsageStatDto {
  couponId: number;
  couponCode: string;
  title: string;
  usedCount: number;
  usageLimit?: number;
  totalDiscountGiven: number;
  totalOrderAmount: number;
  lastUsedAt?: string;
}

@Injectable({ providedIn: 'root' })
export class CouponService {
  private readonly buyerBase = `${environment.apiUrl}/coupons`;
  private readonly adminBase = `${environment.apiUrl}/admin/coupons`;

  constructor(private http: HttpClient) {}

  getAvailable(): Observable<CouponSummaryDto[]> {
    return this.http.get<CouponSummaryDto[]>(`${this.buyerBase}/available`);
  }

  apply(couponCode: string, orderAmount: number): Observable<CouponValidationResultDto> {
    return this.http.post<CouponValidationResultDto>(`${this.buyerBase}/apply`, { couponCode, orderAmount });
  }

  getAllAdmin(): Observable<CouponDto[]> {
    return this.http.get<CouponDto[]>(this.adminBase);
  }

  create(dto: Partial<CouponDto>): Observable<CouponDto> {
    return this.http.post<CouponDto>(this.adminBase, dto);
  }

  update(id: number, dto: Partial<CouponDto>): Observable<CouponDto> {
    return this.http.put<CouponDto>(`${this.adminBase}/${id}`, dto);
  }

  setActive(id: number, isActive: boolean): Observable<CouponDto> {
    return this.http.patch<CouponDto>(`${this.adminBase}/${id}/active`, { isActive });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.adminBase}/${id}`);
  }

  stats(): Observable<CouponUsageStatDto[]> {
    return this.http.get<CouponUsageStatDto[]>(`${this.adminBase}/stats`);
  }
}
