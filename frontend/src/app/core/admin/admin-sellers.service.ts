import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AdminSellerDto {
  sellerId: number;
  userId: number;
  storeName: string;
  storeDescription?: string;
  contactEmail?: string;
  phone?: string;
  isVerified: boolean;
  isActive: boolean;
  ownerEmail: string;
  ownerName: string;
  productCount: number;
  createdAt: string;
}

export interface UpdateAdminSellerDto {
  storeName: string;
  storeDescription?: string;
  contactEmail?: string;
  phone?: string;
}

export interface CreateAdminSellerDto extends UpdateAdminSellerDto {
  userId: number;
}

@Injectable({ providedIn: 'root' })
export class AdminSellersService {
  private readonly base = `${environment.apiUrl}/admin/sellers`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, isActive?: boolean): Observable<AdminSellerDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (isActive !== undefined && isActive !== null) params = params.set('isActive', String(isActive));
    return this.http.get<AdminSellerDto[]>(this.base, { params });
  }

  getById(id: number): Observable<AdminSellerDto> {
    return this.http.get<AdminSellerDto>(`${this.base}/${id}`);
  }

  create(dto: CreateAdminSellerDto): Observable<{ sellerId: number; message: string }> {
    return this.http.post<{ sellerId: number; message: string }>(this.base, dto);
  }

  update(id: number, dto: UpdateAdminSellerDto): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/${id}`, dto);
  }

  setStatus(id: number, isActive: boolean): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.base}/${id}/status`, { isActive });
  }

  setVerified(id: number, isVerified: boolean): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.base}/${id}/verify`, { isVerified });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
