import { NZ_ICONS, NzIconModule } from 'ng-zorro-antd/icon';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router';
import { SiderMenuComponent } from './components/sider-menu/sider-menu.component';
import { HeaderComponent } from './components/header/header.component';

@Component({
  selector: 'app-index.app',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    RouterModule,
    NzLayoutModule,
    NzMenuModule,
    NzIconModule,
    SiderMenuComponent,
    HeaderComponent,
  ],
  templateUrl: './index.app.component.html',
  styleUrl: './index.app.component.scss',
})
export class IndexAppComponent {
  isVisibleMenu = false;

  constructor() {}

  onChangeVisibleMenu(event: boolean) {
    this.isVisibleMenu = event;
  }
}
