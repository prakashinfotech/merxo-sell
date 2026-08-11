import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BrowsingHistoryDto {
  browsingHistoryId: number;
  productId: number;
  productName: string;
  primaryImageUrl?: string;
  basePrice: number;
  salePrice?: number;
  viewedAt: string;
}

@Injectable({ providedIn: 'root' })
export class BrowsingHistoryService {
  private readonly base = `${environment.apiUrl}/browsing-history`;

  constructor(private http: HttpClient) {}

  record(productId: number): Observable<void> {
    return this.http.post<void>(this.base, { productId });
  }

  get(page = 1, pageSize = 12): Observable<BrowsingHistoryDto[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<BrowsingHistoryDto[]>(this.base, { params });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  clear(): Observable<void> {
    return this.http.delete<void>(this.base);
  }
}
