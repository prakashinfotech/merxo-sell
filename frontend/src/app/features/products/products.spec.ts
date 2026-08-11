import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { Products } from './products';
import { ProductService } from './product.service';

const mockPagedResponse = {
  data: [
    { productId: 1, name: 'Laptop', slug: 'laptop', basePrice: 999, isActive: true },
    { productId: 2, name: 'Phone',  slug: 'phone',  basePrice: 499, isActive: true },
  ],
  pagination: { page: 1, pageSize: 20, totalCount: 2, totalPages: 1 }
};

describe('Products component', () => {
  let component: Products;
  let fixture: ComponentFixture<Products>;
  let productService: ProductService;

  beforeEach(async () => {
    const productServiceMock = {
      getAll:  vi.fn(() => of(mockPagedResponse)),
      search:  vi.fn(() => of(mockPagedResponse)),
      suggest: vi.fn(() => of([])),
    } as any;

    await TestBed.configureTestingModule({
      declarations: [Products],
      providers: [
        { provide: ProductService, useValue: productServiceMock },
        {
          provide: ActivatedRoute,
          useValue: { queryParams: of({}) }
        },
        {
          provide: Router,
          useValue: { navigate: vi.fn(), createUrlTree: vi.fn(), serializeUrl: vi.fn() }
        },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(Products);
    component = fixture.componentInstance;
    productService = TestBed.inject(ProductService);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('initialises with loading=true before OnInit', () => {
    expect(component.loading).toBe(true);
  });

  it('ngOnInit() calls productService and populates products', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.products.length).toBe(2);
    expect(component.products[0].name).toBe('Laptop');
  });

  it('pagination is set after load', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.pagination).not.toBeNull();
    expect(component.pagination!.totalCount).toBe(2);
  });

  it('onSidebarFilter() resets page to 1', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    component.filter = { ...component.filter, page: 3 };
    component.onSidebarFilter({ minPrice: 10 });

    expect(component.filter.page).toBe(1);
  });

  it('onSidebarFilter() merges new filter properties', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    component.onSidebarFilter({ minPrice: 50, maxPrice: 200 });

    expect(component.filter.minPrice).toBe(50);
    expect(component.filter.maxPrice).toBe(200);
  });
});
