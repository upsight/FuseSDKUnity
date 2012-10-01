
typedef void* MonoDomain;
typedef void* MonoAssembly;
typedef void* MonoImage;
typedef void* MonoClass;
typedef void* MonoObject;
typedef void* MonoMethodDesc;
typedef void* MonoMethod;
typedef void* MonoString;
typedef int gboolean;
typedef void* gpointer;

MonoDomain *mono_domain_get();
MonoAssembly *mono_domain_assembly_open(MonoDomain *domain, const char *assemblyName);
MonoImage *mono_assembly_get_image(MonoAssembly *assembly);
MonoMethodDesc *mono_method_desc_new(const char *methodString, gboolean useNamespace);
MonoMethodDesc *mono_method_desc_free(MonoMethodDesc *desc);
MonoMethod *mono_method_desc_search_in_image(MonoMethodDesc *methodDesc, MonoImage *image);
MonoObject *mono_runtime_invoke(MonoMethod *method, void *obj, void **params, MonoObject **exc);
MonoClass *mono_class_from_name(MonoImage *image, const char *namespaceString, const char *classnameString);
MonoMethod *mono_class_get_methods(MonoClass*, gpointer* iter);
MonoString *mono_string_new(MonoDomain *domain, const char *text);
char* mono_method_get_name(MonoMethod *method);

MonoDomain* monoDomain;
MonoAssembly* monoAssembly;
MonoImage *monoImage;

void Mono_Initialize()
{
	NSString* assemblyPath = [[[NSBundle mainBundle] bundlePath] stringByAppendingPathComponent:@"Data/Managed/Assembly-CSharp.dll"];
	monoDomain = mono_domain_get();
	monoAssembly = mono_domain_assembly_open(monoDomain, assemblyPath.UTF8String);
	monoImage = mono_assembly_get_image(monoAssembly);
}

void* Mono_GetMethod(const char* name)
{
	MonoMethodDesc* monoMethodDesc = mono_method_desc_new(name, FALSE);
	MonoMethod* monoMethod = mono_method_desc_search_in_image(monoMethodDesc, monoImage);
	mono_method_desc_free(monoMethodDesc);
	
	return monoMethod;
}

void Mono_CallMethod(void* method, void** args)
{
	mono_runtime_invoke((MonoMethod*)method, NULL, args, NULL);
}

void* Mono_NewString(const char* string)
{
	return mono_string_new(monoDomain, string);
}
