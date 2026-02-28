import i18n from "@/i18n";
import {Role} from "@/types/enums";
import {createRouter, createWebHistory} from "vue-router";

import Login from "@/views/Login.vue";
import TwoFactor from "@/views/TwoFactor.vue";
import ForgotPassword from "@/views/ForgotPassword.vue";
import ResetPassword from "@/views/ResetPassword.vue";
import Account from "@/views/shared/Account.vue";

import AthleteSubmission from "@/views/AthleteSubmission.vue";
import Admin from "../views/admin/Admin.vue";
import AdminDashboard from "@/views/admin/AdminDashboard.vue";
import AdminAthleteIndex from "@/views/admin/athletes/AdminAthleteIndex.vue";
import AdminAddAthleteForm from "@/views/admin/athletes/AdminAddAthleteForm.vue";
import AdminTeamIndex from "@/views/admin/teams/AdminTeamIndex.vue";
import AdminAddTeamForm from "@/views/admin/teams/AdminAddTeamForm.vue";
import AdminMemberIndex from "@/views/admin/members/AdminMemberIndex.vue";
import AdminAddMemberForm from "@/views/admin/members/AdminAddMemberForm.vue";
import AdminEditMemberForm from "@/views/admin/members/AdminEditMemberForm.vue";

import Books from "../views/member/Books.vue";
import BookIndex from "@/views/member/BookIndex.vue";
import AddBookForm from "@/views/member/AddBookForm.vue";
import EditBookForm from "@/views/member/EditBookForm.vue";

import {getLocalizedRoutes} from "@/locales/helpers";
import {useUserStore} from "@/stores/userStore";

const router = createRouter({
  // eslint-disable-next-line
  scrollBehavior(to, from, savedPosition) {
    // always scroll to top
    return {top: 0};
  },
  history: createWebHistory(),
  routes: [
    {
      path: i18n.t("routes.login.path"),
      alias: getLocalizedRoutes("routes.login.path"),
      name: "login",
      component: Login,
      meta: {
        title: "routes.login.name"
      }
    },
    {
      path: i18n.t("routes.twoFactor.path"),
      alias: getLocalizedRoutes("routes.twoFactor.path"),
      name: "twoFactor",
      component: TwoFactor,
      meta: {
        title: "routes.twoFactor.name"
      }
    },
    {
      path: i18n.t("routes.forgotPassword.path"),
      alias: getLocalizedRoutes("routes.forgotPassword.path"),
      name: "forgotPassword",
      component: ForgotPassword,
      meta: {
        title: "routes.forgotPassword.name"
      }
    },
    {
      path: i18n.t("routes.resetPassword.path"),
      alias: getLocalizedRoutes("routes.resetPassword.path"),
      name: "resetPassword",
      component: ResetPassword,
      props: (route) => ({userId: route.query.userId, token: route.query.token}),
      meta: {
        title: "routes.resetPassword.name"
      }
    },
    {
      path: i18n.t("routes.account.path"),
      alias: getLocalizedRoutes("routes.account.path"),
      name: "account",
      component: Account,
      meta: {
        title: "routes.account.name"
      }
    },
    {
      // Route publique — pas de requiredRole, le guard laisse passer sans authentification
      path: i18n.t("routes.athleteForm.path"),
      name: "athleteForm",
      component: AthleteSubmission,
      meta: {
        title: "routes.athleteForm.name"
      }
    },
    {
      path: i18n.t("routes.admin.path"),
      name: "admin",
      component: Admin,
      meta: {
        requiredRole: Role.Admin,
        noLinkInBreadcrumbs: true,
      },
      children: [
        {
          path: i18n.t("routes.admin.children.athletes.path"),
          name: "admin.children.athletes",
          component: Admin,
          children: [
            {
              path: "",
              name: "admin.children.athletes.index",
              component: AdminAthleteIndex,
            },
            {
              path: i18n.t("routes.admin.children.athletes.add.path"),
              name: "admin.children.athletes.add",
              component: AdminAddAthleteForm,
            },
          ],
        },
        {
          path: i18n.t("routes.admin.children.teams.path"),
          name: "admin.children.teams",
          component: Admin,
          children: [
            {
              path: "",
              name: "admin.children.teams.index",
              component: AdminTeamIndex,
            },
            {
              path: i18n.t("routes.admin.children.teams.add.path"),
              name: "admin.children.teams.add",
              component: AdminAddTeamForm,
            },
          ],
        },
        {
          path: i18n.t("routes.admin.children.members.path"),
          name: "admin.children.members",
          component: Admin,
          children: [
            {
              path: "",
              name: "admin.children.members.index",
              component: AdminMemberIndex,
            },
            {
              path: i18n.t("routes.admin.children.members.add.path"),
              name: "admin.children.members.add",
              component: AdminAddMemberForm,
            },
            {
              path: i18n.t("routes.admin.children.members.edit.path"),
              alias: i18n.t("routes.admin.children.members.edit.path"),
              name: "admin.children.members.edit",
              component: AdminEditMemberForm,
              props: true
            },
          ],
        }
      ]
    },
    {
      path: i18n.t("routes.admin.children.dashboard.fullPath"),
      name: "admin.children.dashboard",
      component: AdminDashboard,
      meta: {
        requiredRole: Role.Admin,
        title: "routes.admin.children.dashboard.name",
      }
    },
    {
      path: i18n.t("routes.books.path"),
      alias: getLocalizedRoutes("routes.books.path"),
      name: "books",
      component: Books,
      meta: {
        requiredRole: Role.Member,
        title: "routes.books.name"
      },
      children: [
        {
          path: "",
          name: "books.index",
          component: BookIndex,
          meta: {
            title: "routes.books.name"
          }
        },
        {
          path: i18n.t("routes.books.children.add.path"),
          alias: getLocalizedRoutes("routes.books.children.add.path"),
          name: "books.children.add",
          component: AddBookForm,
          meta: {
            title: "routes.books.children.add.name"
          }
        },
        {
          path: i18n.t("routes.books.children.edit.path"),
          alias: getLocalizedRoutes("routes.books.children.edit.path"),
          name: "books.children.edit",
          component: EditBookForm,
          props: true,
          meta: {
            title: "routes.books.children.edit.name"
          }
        }
      ]
    },
  ]
});

// eslint-disable-next-line
router.beforeEach(async (to, from) => {
  const userStore = useUserStore()

  // Handle root path redirect
  if (to.path === "/") {
    if (userStore.user.email)
      return { name: "account" };
    return { name: "login" };
  }

  if (!to.meta.requiredRole)
    return;

  const isRoleArray = Array.isArray(to.meta.requiredRole)
  const doesNotHaveGivenRole = !isRoleArray && !userStore.hasRole(to.meta.requiredRole as Role);
  const hasNoRoleAmongRoleList = isRoleArray && !userStore.hasOneOfTheseRoles(to.meta.requiredRole as Role[]);
  if (doesNotHaveGivenRole || hasNoRoleAmongRoleList) {
    return {
      name: "account",
    };
  }
});

export const Router = router;
