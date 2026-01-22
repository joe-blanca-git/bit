import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NzMenuModule } from 'ng-zorro-antd/menu';

@Component({
  selector: 'app-sider-menu',
  standalone: true,
  imports: [CommonModule, NzMenuModule, RouterModule],
  templateUrl: './sider-menu.component.html',
  styleUrls: ['./sider-menu.component.scss'],
})
export class SiderMenuComponent {
  @Input() collapsed: boolean = false;

  theme: 'dark' | 'light' = 'light';

  menu: any = [];

  constructor() {}

  ngOnInit() {
    // this.themeService.theme$.subscribe(theme => {
    //   this.theme = theme;
    // });
    this.getMenu();
  }

  getMenu() {
    // const _menu = localStorage.getItem('BIT.user');

    // if (_menu) {
    //   const menuData = JSON.parse(_menu);
    //   const menus = menuData.claims.menu;
    //   this.menu = menus;
    // } else {
    // }

    this.menu = [
      {
        name: 'Financeiro',
        icon: 'dollar',
        submenus: [
          {
            name: 'Receitas',
            route: '/financial/income',
          },
          {
            name: 'Despesas',
            route: '',
          },
           {
            name: 'Fluxo de Caixa',
            route: '',
          },
           {
            name: 'Planejamento',
            route: '',
          },
        ],
      },
      {
        name: 'Pedidos',
        icon: 'shop',
        submenus: [
          {
            name: 'Compras',
            route: '',
          },
          {
            name: 'Vendas',
            route: '',
          },
        ],
      },
      {
        name: 'Estoque',
        icon: 'dropbox',
        submenus: [
          {
            name: 'Produtos',
            route: '',
          },
          {
            name: 'Movimentações',
            route: '',
          },
        ],
      },
    ];
  }
}
