import { Injectable } from '@angular/core';
import { LocalStorageUtils } from '../utils/localstorage';

@Injectable({
  providedIn: 'root',
})
export class NavigationService {
  private localStorageUtils = new LocalStorageUtils();

  constructor() {}

  getMenuAllowed(): any[] {
    const menu: any[] = [];
    const menuAllowedString = this.localStorageUtils.getMenuAllowed();

    if (!menuAllowedString) {
      return menu;
    }

    let data: any[];

    try {
      data = JSON.parse(menuAllowedString);
    } catch {
      return menu;
    }

    menu.push({
      title: 'Home',
      description: 'Pagina Inicial',
      icon: 'home',
      route: '/home',
      submenus: [],
      visible: true,
      active: false,
    });

    if (Array.isArray(data) && data.length > 0) {
      for (const record of data) {
        const { items, ...rest } = record;

        const currentSection = {
          ...rest,
          submenus: Array.isArray(items) ? items : [],
        };

        menu.push(currentSection);
      }
    }
    return menu;
  }
}
