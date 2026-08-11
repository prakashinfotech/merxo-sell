import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { Orders } from './orders';
import { AdminOrdersService } from '../../../core/admin/admin-orders.service';
import { DialogService } from '../../../core/dialog/dialog.service';
import { ToastService } from '../../../core/toast/toast.service';

describe('Orders', () => {
  let component: Orders;
  let fixture: ComponentFixture<Orders>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Orders],
      providers: [
        { provide: AdminOrdersService, useValue: { getAll: () => of({ data: [], pagination: {} }) } },
        { provide: DialogService, useValue: { confirm: () => of(false) } },
        { provide: ToastService, useValue: { success: () => {}, error: () => {} } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(Orders);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
