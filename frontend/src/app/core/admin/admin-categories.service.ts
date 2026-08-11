import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AdminCategoryDto {
  categoryId: number;
  parentCategoryId?: number;
  parentName?: string;
  name: string;
  slug: string;
  imageUrl?: string;
  sortOrder: number;
  isActive: boolean;
  productCount: number;
  subCategoryCount: number;
}

export interface CreateCategoryDto {
  name: string;
  slug?: string;
  imageUrl?: string;
  parentCategoryId?: number | null;
  sortOrder: number;
  isActive: boolean;
}

export interface UpdateCategoryDto extends CreateCategoryDto {}

@Injectable({ providedIn: 'root' })
export class AdminCategoriesService {
  private readonly base = `${environment.apiUrl}/admin/categories`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AdminCategoryDto[]> {
    return this.http.get<AdminCategoryDto[]>(this.base);
  }

  getById(id: number): Observable<AdminCategoryDto> {
    return this.http.get<AdminCategoryDto>(`${this.base}/${id}`);
  }

  create(dto: CreateCategoryDto): Observable<AdminCategoryDto> {
    return this.http.post<AdminCategoryDto>(this.base, dto);
  }

  update(id: number, dto: UpdateCategoryDto): Observable<AdminCategoryDto> {
    return this.http.put<AdminCategoryDto>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
