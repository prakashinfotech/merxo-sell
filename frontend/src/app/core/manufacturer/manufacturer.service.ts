import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ManufacturerDto {
  manufacturerId: number;
  name: string;
  contactEmail?: string;
  phone?: string;
  address?: string;
  country?: string;
  website?: string;
  isActive: boolean;
  createdAt: string;
  productCount: number;
}

export interface CreateManufacturerDto {
  name: string;
  contactEmail?: string;
  phone?: string;
  address?: string;
  country?: string;
  website?: string;
}

export interface UpdateManufacturerDto extends CreateManufacturerDto {
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class ManufacturerService {
  private readonly base = `${environment.apiUrl}/manufacturers`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ManufacturerDto[]> {
    return this.http.get<ManufacturerDto[]>(this.base);
  }

  getById(id: number): Observable<ManufacturerDto> {
    return this.http.get<ManufacturerDto>(`${this.base}/${id}`);
  }

  create(dto: CreateManufacturerDto): Observable<ManufacturerDto> {
    return this.http.post<ManufacturerDto>(this.base, dto);
  }

  update(id: number, dto: UpdateManufacturerDto): Observable<ManufacturerDto> {
    return this.http.put<ManufacturerDto>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
