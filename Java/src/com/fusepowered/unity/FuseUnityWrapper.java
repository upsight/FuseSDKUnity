package com.fusepowered.unity;

/**
 * This class loads the Java Native Interface (JNI)
 * library, 'libFuseUnityWrapper.so', and provides access to the
 * exposed C functions.
 * The library is packaged and installed with the application.
 * See the C file, /jni/FuseUnityWrapper.c file for the
 * implementations of the native methods. 
 * 
 * For more information on JNI, see: http://java.sun.com/docs/books/jni/
 */
public class FuseUnityWrapper
{
	/**
	 * An example native method.  See the library function,
	 * <code>Java_com_fusepowered_unity_FuseUnityWrapper_fuseunitywrapperNative</code>
	 * for the implementation.
	 */
	static public native void fuseunitywrapperNative();

	/* This is the static constructor used to load the
	 * 'FuseUnityWrapper' library when the class is
	 * loaded.
	 */
	static {
		System.loadLibrary("FuseUnityWrapper");
	}
}
