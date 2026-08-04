import { createI18n } from "vue-i18n";
import generalEn from "@/shared/locales/messages/en/general.json";
import authEn from "@/shared/locales/messages/en/auth.json";
import catalogEn from "@/shared/locales/messages/en/catalog.json";
import inventoryEn from "@/shared/locales/messages/en/inventory.json";
import orderingEn from "@/shared/locales/messages/en/ordering.json";
import usersEn from "@/shared/locales/messages/en/users.json";
import rolesEn from "@/shared/locales/messages/en/roles.json";
import locationEn from "@/shared/locales/messages/en/location.json";
import profileEn from "@/shared/locales/messages/en/profile.json";
import errorEn from "@/shared/locales/messages/en/error.json";
import reportsEn from "@/shared/locales/messages/en/reports.json";

export type MessageSchema = typeof generalEn;

const i18n = createI18n({
  legacy: false,
  locale: "en",
  fallbackLocale: "en",
  messages: {
    en: {
      ...generalEn,
      auth: authEn,
      catalog: catalogEn,
      inventory: inventoryEn,
      ordering: orderingEn,
      users: usersEn,
      roles: rolesEn,
      location: locationEn,
      profile: profileEn,
      error: errorEn,
      reports: reportsEn,
    },
  },
});

export default i18n;
