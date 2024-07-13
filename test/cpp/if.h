#define LOADED_IF_H

#if defined(A) && defined(B)

# if defined(B) || defined(C)

	#if defined(C) && defined(D) || !defined(E)
		#define hi
	#elif E
		#define hello
	#endif

# endif

#endif