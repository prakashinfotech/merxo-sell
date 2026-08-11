import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { Sellers } from './sellers';
import { AdminSellersService } from '../../../core/admin/admin-sellers.service';
import { DialogService } from '../../../core/dialog/dialog.service';

describe('Sellers', () => {
  let component: Sellers;
  let fixture: ComponentFixture<Sellers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Sellers],
      providers: [
        { provide: AdminSellersService, useValue: { getAll: () => of([]) } },
        { provide: DialogService, useValue: { confirm: () => of(false) } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(Sellers);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
