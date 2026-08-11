import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AdminCurrencyDto {
  currencyCode: string;
  currencyName: string;
  rateToCad: number;
  symbol: string;
  isActive: boolean;
  lastUpdated: string;
}

export interface CreateCurrencyDto {
  currencyCode: string;
  currencyName: string;
  rateToCad: number;
  symbol: string;
  isActive: boolean;
}

export interface UpdateCurrencyDto {
  currencyName: string;
  rateToCad: number;
  symbol: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminCurrenciesService {
  private readonly base = `${environment.apiUrl}/admin/rates`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AdminCurrencyDto[]> {
    return this.http.get<AdminCurrencyDto[]>(this.base);
  }

  getByCode(code: string): Observable<AdminCurrencyDto> {
    return this.http.get<AdminCurrencyDto>(`${this.base}/${code}`);
  }

  create(dto: CreateCurrencyDto): Observable<AdminCurrencyDto> {
    return this.http.post<AdminCurrencyDto>(this.base, dto);
  }

  update(code: string, dto: UpdateCurrencyDto): Observable<AdminCurrencyDto> {
    return this.http.put<AdminCurrencyDto>(`${this.base}/${code}`, dto);
  }

  delete(code: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${code}`);
  }
}
