// Bridge class that exposes the Icons.Regular/Filled.SizeXX.Yyy pattern
// required because FluentUI v5 no longer ships a public "Icons" wrapper class.
using R12 = Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size12;
using R16 = Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size16;
using R20 = Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20;
using R48 = Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size48;

namespace dWebShop.Admin;

public static class Icons
{
    public static class Regular
    {
        public static class Size12
        {
            public class Checkmark : R12.Checkmark { }
        }

        public static class Size16
        {
            public class Add : R16.Add { }
            public class ArrowDownload : R16.ArrowDownload { }
            public class ArrowLeft : R16.ArrowLeft { }
            public class Calendar : R16.Calendar { }
            public class Checkmark : R16.Checkmark { }
            public class CheckmarkCircle : R16.CheckmarkCircle { }
            public class ChevronLeft : R16.ChevronLeft { }
            public class ChevronRight : R16.ChevronRight { }
            public class Dismiss : R16.Dismiss { }
            public class DismissCircle : R16.DismissCircle { }
            public class Print : R16.Print { }
            public class DocumentText : R16.DocumentText { }
            public class Edit : R16.Edit { }
            public class Filter : R16.Filter { }
            public class Mail : R16.Mail { }
            public class MoreHorizontal : R16.MoreHorizontal { }
            public class Payment : R16.Payment { }
            public class Phone : R16.Phone { }
            public class Search : R16.Search { }
        }

        public static class Size20
        {
            public class Box : R20.Box { }
            public class Building : R20.Building { }
            public class BuildingShop : R20.BuildingShop { }
            public class Cart : R20.Cart { }
            public class ClipboardTask : R20.ClipboardTask { }
            public class Dismiss : R20.Dismiss { }
            public class DocumentText : R20.DocumentText { }
            public class Home : R20.Home { }
            public class Money : R20.Money { }
            public class News : R20.News { }
            public class People : R20.People { }
            public class Search : R20.Search { }
            public class Settings : R20.Settings { }
            public class Tag : R20.Tag { }
            public class TextBulletListTree : R20.TextBulletListTree { }
        }

        public static class Size48
        {
            public class Building : R48.Building { }
            public class ClipboardTask : R48.Clipboard { }
            public class People : R48.People { }
            public class Tag : R48.Tag { }
        }
    }
}
