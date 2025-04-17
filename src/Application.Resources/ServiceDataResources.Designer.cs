namespace Stellantis.ProjectName.Application.Resources {

    /// <summary>
    ///   Uma classe de recurso de tipo de alta segurança, para pesquisar cadeias de caracteres localizadas etc.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ServiceDataResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ServiceDataResources() {
        }
        
        /// <summary>
        ///   Retorna a instância de ResourceManager armazenada em cache usada por essa classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Stellantis.ProjectName.Application.Resources.ServiceDataResources", typeof(ServiceDataResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Substitui a propriedade CurrentUICulture do thread atual para todas as
        ///   pesquisas de recursos que usam essa classe de recurso de tipo de alta segurança.
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
        ///   Consulta uma cadeia de caracteres localizada semelhante a Service already exists..
        /// </summary>
        public static string ServiceAlreadyExists {
            get {
                return ResourceManager.GetString("ServiceAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Consulta uma cadeia de caracteres localizada semelhante a Service cannot be null..
        /// </summary>
        public static string ServiceCannotBeNull {
            get {
                return ResourceManager.GetString("ServiceCannotBeNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Consulta uma cadeia de caracteres localizada semelhante a Service description cannot exceed 255 characters..
        /// </summary>
        public static string ServiceDescriptionLength {
            get {
                return ResourceManager.GetString("ServiceDescriptionLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Consulta uma cadeia de caracteres localizada semelhante a Service invalid application id..
        /// </summary>
        public static string ServiceInvalidApplicationId {
            get {
                return ResourceManager.GetString("ServiceInvalidApplicationId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Consulta uma cadeia de caracteres localizada semelhante a Service name is required..
        /// </summary>
        public static string ServiceNameIsRequired {
            get {
                return ResourceManager.GetString("ServiceNameIsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Consulta uma cadeia de caracteres localizada semelhante a Service name must be between 3 and 50 characters..
        /// </summary>
        public static string ServiceNameLength {
            get {
                return ResourceManager.GetString("ServiceNameLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Consulta uma cadeia de caracteres localizada semelhante a Service not found..
        /// </summary>
        public static string ServiceNotFound {
            get {
                return ResourceManager.GetString("ServiceNotFound", resourceCulture);
            }
        }
    }
}
