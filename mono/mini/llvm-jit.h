/**
 * \file
 * LLVM backend
 *
 * Authors:
 *   Zoltan Varga (vargaz@gmail.com)
 *
 * (C) 2009 Novell, Inc.
 */

#ifndef __MONO_LLVM_JIT_H__
#define __MONO_LLVM_JIT_H__

#include <glib.h>

#include "llvm-c/Core.h"
#include "llvm-c/ExecutionEngine.h"

#ifdef HAVE_UNWIND_H
#include <unwind.h>
#endif

/* These can't go into mini-<ARCH>.h since thats not included into llvm-jit.cpp */
#if defined(TARGET_AMD64) && defined(TARGET_OSX)
#define MONO_ARCH_LLVM_JIT_SUPPORTED 1
#elif defined(TARGET_X86) && defined(TARGET_OSX)
#define MONO_ARCH_LLVM_JIT_SUPPORTED 1
#endif

G_BEGIN_DECLS

typedef unsigned char * (AllocCodeMemoryCb) (LLVMValueRef function, int size);
typedef void (FunctionEmittedCb) (LLVMValueRef function, void *start, void *end);
typedef void (ExceptionTableCb) (void *data);
typedef char* (DlSymCb) (const char *name, void **symbol);

typedef void* MonoEERef;

MonoEERef
mono_llvm_create_ee (LLVMModuleProviderRef MP, AllocCodeMemoryCb *alloc_cb, FunctionEmittedCb *emitted_cb, ExceptionTableCb *exception_cb, DlSymCb *dlsym_cb, LLVMExecutionEngineRef *ee);

void
mono_llvm_dispose_ee (MonoEERef *mono_ee);

gpointer
mono_llvm_compile_method (MonoEERef mono_ee, LLVMValueRef method, int nvars, LLVMValueRef *callee_vars, gpointer *callee_addrs, gpointer *eh_frame);

void
mono_llvm_optimize_method (MonoEERef mono_ee, LLVMValueRef method);

void
mono_llvm_set_unhandled_exception_handler (void);

G_END_DECLS

#endif /* __MONO_LLVM_JIT_H__ */
