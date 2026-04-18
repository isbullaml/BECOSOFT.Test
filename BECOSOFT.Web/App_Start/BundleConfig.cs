using System.Web.Optimization;

namespace BECOSOFT.Web {
    public class BundleConfig {
        public static void RegisterBundles(BundleCollection bundles) {
            bundles.Add(JqueryJsBundle);
            bundles.Add(DashboardJsBundle);
            bundles.Add(BootstrapJsBundle);
            bundles.Add(GeneralJsBundle);
            bundles.Add(DocumentJsBundle);
            bundles.Add(ArticleJsBundle);
            bundles.Add(StyleBundle);
            bundles.Add(ContentBuilderStyleBundle);
            bundles.Add(HomeTemplateBuilderStyleBundle);
        }

        private static Bundle JqueryJsBundle =>
            new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery.sumoselect.min.js",
                "~/Scripts/jquery.multi-select.js",
                "~/Scripts/jquery.multi-select.extensions.js",
                "~/Scripts/jquery.ui.touch-punch.min.js",
                "~/Scripts/jquery.quicksearch.js",
                "~/Scripts/moment.js",
                "~/Scripts/moment-with-locales.js",
                "~/Scripts/image-utilities.js",
                "~/Scripts/checkUrl.js",
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/becosoft.suggestags.js",
                "~/Scripts/jquery.dirtyforms.min.js");

        private static Bundle DashboardJsBundle =>
            new ScriptBundle("~/bundles/dashboard").Include(
                "~/Scripts/Chart.min.js",
                "~/Scripts/dashboard.js",
                "~/Scripts/powerbi.min.js");

        private static Bundle BootstrapJsBundle =>
            new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.js",
                "~/Scripts/respond.js");

        private static Bundle GeneralJsBundle =>
            new ScriptBundle("~/bundles/general").Include(
                "~/Scripts/general.js",
                "~/Scripts/menu.js",
                "~/Scripts/becosoft.js",
                "~/Scripts/condition-filter.js",
                "~/Scripts/image-utilities.js",
                "~/Scripts/loadingIcon.js",
                "~/Scripts/nonmodaldialog.js",
                "~/Scripts/numberInput.js",
                "~/Scripts/topHorizontalScrollbar.js",
                "~/Scripts/slider.js",
                "~/Scripts/select2.min.js",
                "~/froala-editor/js/froala_editor.pkgd.min.js",
                "~/froala-editor/js/custom-plugins.js",
                "~/Scripts/froala.js");

        private static Bundle DocumentJsBundle =>
            new ScriptBundle("~/bundles/document").Include(
                "~/Scripts/Document/functions.js",
                "~/Scripts/Document/contact.js",
                "~/Scripts/Document/details.js",
                "~/Scripts/Document/keyBindings.js",
                "~/Scripts/Document/modalKeyBindings.js",
                "~/Scripts/Document/events.js",
                "~/Scripts/Document/DocumentDetailImportWizard.js");

        private static Bundle ArticleJsBundle =>
            new ScriptBundle("~/bundles/article").Include("~/Scripts/Article/ArticleSearchWizard.js");

        private static Bundle StyleBundle =>
            new StyleBundle("~/Content/style").Include(
                "~/Content/Site.css",
                "~/Content/Staging.css",
                "~/Content/sumoselect.min.css",
                "~/Content/sumoselect-custom.css",
                "~/Content/multi-select.css",
                "~/Content/select2.min.css",
                "~/Content/becosoft.suggestags.css",
                "~/froala-editor/css/froala_editor.pkgd.min.css",
                "~/Content/jquery-ui.css",
                "~/Content/BecosoftStyle/main.css");

        private static Bundle ContentBuilderStyleBundle =>
            new StyleBundle("~/Content/contentbuilderstyle").Include(
                "~/Content/contentbuilder.css");

        private static Bundle HomeTemplateBuilderStyleBundle =>
            new StyleBundle("~/Content/hometemplatebuilderstyle").Include(
                "~/Content/hometemplatebuilder.css");
    }
}