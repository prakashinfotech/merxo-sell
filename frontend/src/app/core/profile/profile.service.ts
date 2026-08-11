import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AddressDto {
  addressId: number;
  fullName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string;
  label?: string | null;
  isDefault: boolean;
}

export interface PaymentMethodDto {
  id: number;
  type: string;
  label: string;
  subLabel?: string;
  isDefault: boolean;
  createdAt: string;
}

export interface UserProfileDto {
  userId: number;
  fullName: string;
  email: string;
  phone?: string;
  preferredCurrency: string;
  country: string;
  createdAt: string;
  addresses: AddressDto[];
  paymentMethods: PaymentMethodDto[];
}

export interface UpdateProfileDto {
  fullName: string;
  phone?: string;
  preferredCurrency: string;
  country: string;
}

export interface AddressCreateUpdateDto {
  fullName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string;
  label?: string | null;
  isDefault?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private apiUrl = `${environment.apiUrl}/profile`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(this.apiUrl);
  }

  updateProfile(dto: UpdateProfileDto): Observable<UserProfileDto> {
    return this.http.put<UserProfileDto>(this.apiUrl, dto);
  }

  /** GET /api/profile/addresses */
  getAddresses(): Observable<AddressDto[]> {
    return this.http.get<AddressDto[]>(`${this.apiUrl}/addresses`);
  }

  addAddress(dto: AddressCreateUpdateDto): Observable<AddressDto> {
    return this.http.post<AddressDto>(`${this.apiUrl}/addresses`, dto);
  }

  updateAddress(id: number, dto: AddressCreateUpdateDto): Observable<AddressDto> {
    return this.http.put<AddressDto>(`${this.apiUrl}/addresses/${id}`, dto);
  }

  deleteAddress(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/addresses/${id}`);
  }

  setDefaultAddress(id: number): Observable<AddressDto> {
    return this.http.patch<AddressDto>(`${this.apiUrl}/addresses/${id}/default`, {});
  }

  changePassword(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, dto);
  }

  getSellerStore(): Observable<any> {
    return this.http.get(`${this.apiUrl}/seller-store`);
  }

  updateSellerStore(dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/seller-store`, dto);
  }

  addPaymentMethod(dto: any): Observable<PaymentMethodDto> {
    return this.http.post<PaymentMethodDto>(`${this.apiUrl}/payments`, dto);
  }

  deletePaymentMethod(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/payments/${id}`);
  }

  setDefaultPaymentMethod(id: number): Observable<PaymentMethodDto> {
    return this.http.patch<PaymentMethodDto>(`${this.apiUrl}/payments/${id}/default`, {});
  }
}
