#pragma once

#define LINK_TO_MOGRE 0



#if LINK_TO_MOGRE
#ifdef _DEBUG
#pragma comment(lib, "../../../lib/Debug/mogre_d.lib")
#else
#pragma comment(lib, "../../../lib/Release/mogre.lib")
#endif
#endif
