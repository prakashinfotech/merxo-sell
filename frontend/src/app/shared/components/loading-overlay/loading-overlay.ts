import { Component } from '@angular/core';
import { LoadingService } from '../../../core/loading/loading.service';

@Component({
  selector: 'app-loading-overlay',
  templateUrl: './loading-overlay.html',
  styleUrls: ['./loading-overlay.scss'],
  standalone: false,
})
export class LoadingOverlay {
  constructor(public loadingService: LoadingService) {}
}
