﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Avs.StaticSiteHosting.ContentHost.Properties {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Avs.StaticSiteHosting.ContentHost.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на &lt;html lang=&quot;&quot;en&quot;&quot;&gt;
        /// &lt;head&gt;
        ///  &lt;title&gt;Oops, there is a problem!&lt;/title&gt;
        ///  &lt;link rel=&quot;&quot;stylesheet&quot;&quot; href=&quot;&quot;/styles.css&quot;&quot; type=&quot;&quot;text/css&quot;&quot; /&gt;
        ///. &lt;/head&gt;
        /// &lt;body&gt;
        ///  &lt;div&gt;
        ///   &lt;div class=&quot;&quot;error-title&quot;&quot;&gt;The resource requested can not be delivered.&lt;/div&gt;
        ///   &lt;div class=&quot;&quot;error-subtitle&quot;&quot;&gt;Please find the reason why it happened: &lt;/div&gt;
        ///   &lt;div class=&quot;&quot;error-description&quot;&quot;&gt;{0}&lt;/div&gt;
        ///  &lt;/div&gt;
        /// &lt;/body&gt;
        ///&lt;/html&gt;.
        /// </summary>
        public static string ErorPageTemplate {
            get {
                return ResourceManager.GetString("ErorPageTemplate", resourceCulture);
            }
        }
    }
}
