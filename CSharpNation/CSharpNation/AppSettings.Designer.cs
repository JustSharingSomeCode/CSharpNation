﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSharpNation {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class AppSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static AppSettings defaultInstance = ((AppSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new AppSettings())));
        
        public static AppSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("70")]
        public int Background_Dim {
            get {
                return ((int)(this["Background_Dim"]));
            }
            set {
                this["Background_Dim"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("500")]
        public int N_Particles {
            get {
                return ((int)(this["N_Particles"]));
            }
            set {
                this["N_Particles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("120")]
        public int Background_Change_Seconds {
            get {
                return ((int)(this["Background_Change_Seconds"]));
            }
            set {
                this["Background_Change_Seconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool Auto_Change_Background {
            get {
                return ((bool)(this["Auto_Change_Background"]));
            }
            set {
                this["Auto_Change_Background"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool Wave_Enhancements {
            get {
                return ((bool)(this["Wave_Enhancements"]));
            }
            set {
                this["Wave_Enhancements"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.5")]
        public double Logo_Left_Offset {
            get {
                return ((double)(this["Logo_Left_Offset"]));
            }
            set {
                this["Logo_Left_Offset"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.5")]
        public double Logo_Right_Offset {
            get {
                return ((double)(this["Logo_Right_Offset"]));
            }
            set {
                this["Logo_Right_Offset"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.5")]
        public double Logo_Top_Offset {
            get {
                return ((double)(this["Logo_Top_Offset"]));
            }
            set {
                this["Logo_Top_Offset"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.5")]
        public double Logo_Bottom_Offset {
            get {
                return ((double)(this["Logo_Bottom_Offset"]));
            }
            set {
                this["Logo_Bottom_Offset"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool Enable_Background_Movement {
            get {
                return ((bool)(this["Enable_Background_Movement"]));
            }
            set {
                this["Enable_Background_Movement"] = value;
            }
        }
    }
}
