import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { Currencies } from './currencies';
import { AdminCurrenciesService } from '../../../core/admin/admin-currencies.service';
import { DialogService } from '../../../core/dialog/dialog.service';

describe('Currencies', () => {
  let component: Currencies;
  let fixture: ComponentFixture<Currencies>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Currencies],
      providers: [
        { provide: AdminCurrenciesService, useValue: { getAll: () => of([]) } },
        { provide: DialogService, useValue: { confirm: () => of(false) } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(Currencies);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
