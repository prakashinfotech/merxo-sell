import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { SellerProducts } from './seller-products';
import { SellerService } from '../../../core/seller/seller.service';
import { MatDialog } from '@angular/material/dialog';
import { provideRouter } from '@angular/router';

describe('SellerProducts', () => {
  let component: SellerProducts;
  let fixture: ComponentFixture<SellerProducts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SellerProducts],
      providers: [
        provideRouter([]),
        { provide: SellerService, useValue: { getProducts: () => of([]) } },
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => of(null) }) } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerProducts);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
