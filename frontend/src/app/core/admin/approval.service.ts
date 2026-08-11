import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductApprovalDto {
  productId: number;
  sellerId: number;
  sellerName: string;
  name: string;
  basePrice: number;
  salePrice?: number;
  status: string;
  approvalNote?: string;
}

export interface ApproveProductDto {
  isApproved: boolean;
  approvalNote?: string;
}

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private apiUrl = `${environment.apiUrl}/approvals`;

  constructor(private http: HttpClient) {}

  getPendingProducts(): Observable<ProductApprovalDto[]> {
    return this.http.get<ProductApprovalDto[]>(`${this.apiUrl}/pending`);
  }

  approveProduct(productId: number, isApproved: boolean, note?: string): Observable<any> {
    const dto: ApproveProductDto = { isApproved, approvalNote: note };
    return this.http.post(`${this.apiUrl}/${productId}/approve`, dto);
  }
}
