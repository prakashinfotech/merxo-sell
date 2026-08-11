import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Footer {
  readonly currentYear = new Date().getFullYear();
}
