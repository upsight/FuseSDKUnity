
void Mono_Initialize();
void* Mono_GetMethod(const char* name);
void Mono_CallMethod(void* method, void** args);
void* Mono_NewString(const char* string);
