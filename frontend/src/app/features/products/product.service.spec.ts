import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProductService } from './product.service';

const mockPaged = {
  data: [{ productId: 1, name: 'Laptop', slug: 'laptop', basePrice: 999, isActive: true }],
  pagination: { page: 1, pageSize: 20, totalCount: 1, totalPages: 1 }
};

describe('ProductService', () => {
  let service: ProductService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(ProductService);
    http    = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('getAll() GETs /products without params when no filter', () => {
    service.getAll().subscribe();

    const req = http.expectOne(r => r.url === 'http://localhost:5000/api/products');
    expect(req.request.method).toBe('GET');
    req.flush(mockPaged);
  });

  it('getAll() passes categoryId as query param', () => {
    service.getAll({ categoryId: 5 }).subscribe();

    const req = http.expectOne(r => r.urlWithParams.includes('categoryId=5'));
    expect(req.request.params.get('categoryId')).toBe('5');
    req.flush(mockPaged);
  });

  it('getById() GETs /products/:id', () => {
    service.getById(42).subscribe();

    const req = http.expectOne('http://localhost:5000/api/products/42');
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('search() GETs /products/search with query param q', () => {
    service.search({ q: 'laptop' }).subscribe();

    const req = http.expectOne(r => r.urlWithParams.includes('q=laptop'));
    expect(req.request.url).toBe('http://localhost:5000/api/products/search');
    req.flush(mockPaged);
  });

  it('suggest() GETs /products/suggest with q param', () => {
    service.suggest('lap').subscribe();

    const req = http.expectOne(r => r.urlWithParams.includes('q=lap'));
    expect(req.request.url).toBe('http://localhost:5000/api/products/suggest');
    req.flush([]);
  });

  it('getAll() passes minPrice and maxPrice params when provided', () => {
    service.getAll({ minPrice: 10, maxPrice: 100 }).subscribe();

    const req = http.expectOne(r => r.urlWithParams.includes('minPrice=10'));
    expect(req.request.params.get('maxPrice')).toBe('100');
    req.flush(mockPaged);
  });
});
