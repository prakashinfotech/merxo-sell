import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CustomerDto {
  userId: number;
  fullName: string;
  email: string;
  phone?: string;
  preferredCurrency: string;
  isActive: boolean;
  orderCount: number;
  totalSpentCAD: number;
  addressCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CustomerAddressDto {
  addressId: number;
  fullName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode: string;
  country: string;
  phone: string;
  isDefault: boolean;
}

export interface CustomerDetailDto extends CustomerDto {
  lastOrderStatus?: string;
  lastOrderAt?: string;
  addresses: CustomerAddressDto[];
}

export interface UpdateCustomerDto {
  fullName: string;
  phone?: string;
  preferredCurrency: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminCustomersService {
  private readonly base = `${environment.apiUrl}/admin/customers`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, isActive?: boolean): Observable<CustomerDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (isActive !== undefined && isActive !== null) params = params.set('isActive', String(isActive));
    return this.http.get<CustomerDto[]>(this.base, { params });
  }

  getById(id: number): Observable<CustomerDetailDto> {
    return this.http.get<CustomerDetailDto>(`${this.base}/${id}`);
  }

  update(id: number, dto: UpdateCustomerDto): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/${id}`, dto);
  }

  setStatus(id: number, isActive: boolean): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.base}/${id}/status`, { isActive });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
