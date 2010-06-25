#pragma once

#define LINK_TO_MOGRE 1



#if LINK_TO_MOGRE
#ifdef _DEBUG
#pragma comment(lib, "../../../lib/Debug/mogre.lib")
#else
#pragma comment(lib, "../../../lib/Release/mogre.lib")
#endif
#endif
