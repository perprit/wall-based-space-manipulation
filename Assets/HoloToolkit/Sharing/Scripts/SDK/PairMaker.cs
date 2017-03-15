/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 3.0.2
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */

namespace HoloToolkit.Sharing {

public class PairMaker : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal PairMaker(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(PairMaker obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~PairMaker() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          SharingClientPINVOKE.delete_PairMaker(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public virtual bool IsReceiver() {
    bool ret = (SwigDerivedClassHasMethod("IsReceiver", swigMethodTypes0) ? SharingClientPINVOKE.PairMaker_IsReceiverSwigExplicitPairMaker(swigCPtr) : SharingClientPINVOKE.PairMaker_IsReceiver(swigCPtr));
    return ret;
  }

  public virtual int GetAddressCount() {
    int ret = SharingClientPINVOKE.PairMaker_GetAddressCount(swigCPtr);
    return ret;
  }

  public virtual XString GetAddress(int index) {
    global::System.IntPtr cPtr = (SwigDerivedClassHasMethod("GetAddress", swigMethodTypes2) ? SharingClientPINVOKE.PairMaker_GetAddressSwigExplicitPairMaker(swigCPtr, index) : SharingClientPINVOKE.PairMaker_GetAddress(swigCPtr, index));
    XString ret = (cPtr == global::System.IntPtr.Zero) ? null : new XString(cPtr, true);
    return ret; 
  }

  public virtual ushort GetPort() {
    ushort ret = (SwigDerivedClassHasMethod("GetPort", swigMethodTypes3) ? SharingClientPINVOKE.PairMaker_GetPortSwigExplicitPairMaker(swigCPtr) : SharingClientPINVOKE.PairMaker_GetPort(swigCPtr));
    return ret;
  }

  public virtual void Update() {
    if (SwigDerivedClassHasMethod("Update", swigMethodTypes4)) SharingClientPINVOKE.PairMaker_UpdateSwigExplicitPairMaker(swigCPtr); else SharingClientPINVOKE.PairMaker_Update(swigCPtr);
  }

  public virtual bool IsReadyToConnect() {
    bool ret = (SwigDerivedClassHasMethod("IsReadyToConnect", swigMethodTypes5) ? SharingClientPINVOKE.PairMaker_IsReadyToConnectSwigExplicitPairMaker(swigCPtr) : SharingClientPINVOKE.PairMaker_IsReadyToConnect(swigCPtr));
    return ret;
  }

  public virtual int GetLocalKey() {
    int ret = (SwigDerivedClassHasMethod("GetLocalKey", swigMethodTypes6) ? SharingClientPINVOKE.PairMaker_GetLocalKeySwigExplicitPairMaker(swigCPtr) : SharingClientPINVOKE.PairMaker_GetLocalKey(swigCPtr));
    return ret;
  }

  public virtual int GetRemoteKey() {
    int ret = (SwigDerivedClassHasMethod("GetRemoteKey", swigMethodTypes7) ? SharingClientPINVOKE.PairMaker_GetRemoteKeySwigExplicitPairMaker(swigCPtr) : SharingClientPINVOKE.PairMaker_GetRemoteKey(swigCPtr));
    return ret;
  }

  public PairMaker() : this(SharingClientPINVOKE.new_PairMaker(), true) {
    SwigDirectorConnect();
  }

  private void SwigDirectorConnect() {
    if (SwigDerivedClassHasMethod("IsReceiver", swigMethodTypes0))
      swigDelegate0 = new SwigDelegatePairMaker_0(SwigDirectorIsReceiver);
    if (SwigDerivedClassHasMethod("GetAddressCount", swigMethodTypes1))
      swigDelegate1 = new SwigDelegatePairMaker_1(SwigDirectorGetAddressCount);
    if (SwigDerivedClassHasMethod("GetAddress", swigMethodTypes2))
      swigDelegate2 = new SwigDelegatePairMaker_2(SwigDirectorGetAddress);
    if (SwigDerivedClassHasMethod("GetPort", swigMethodTypes3))
      swigDelegate3 = new SwigDelegatePairMaker_3(SwigDirectorGetPort);
    if (SwigDerivedClassHasMethod("Update", swigMethodTypes4))
      swigDelegate4 = new SwigDelegatePairMaker_4(SwigDirectorUpdate);
    if (SwigDerivedClassHasMethod("IsReadyToConnect", swigMethodTypes5))
      swigDelegate5 = new SwigDelegatePairMaker_5(SwigDirectorIsReadyToConnect);
    if (SwigDerivedClassHasMethod("GetLocalKey", swigMethodTypes6))
      swigDelegate6 = new SwigDelegatePairMaker_6(SwigDirectorGetLocalKey);
    if (SwigDerivedClassHasMethod("GetRemoteKey", swigMethodTypes7))
      swigDelegate7 = new SwigDelegatePairMaker_7(SwigDirectorGetRemoteKey);
    SharingClientPINVOKE.PairMaker_director_connect(swigCPtr, swigDelegate0, swigDelegate1, swigDelegate2, swigDelegate3, swigDelegate4, swigDelegate5, swigDelegate6, swigDelegate7);
  }

  private bool SwigDerivedClassHasMethod(string methodName, global::System.Type[] methodTypes) {
    global::System.Reflection.MethodInfo methodInfo = this.GetType().GetMethod(methodName, global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance, null, methodTypes, null);
    bool hasDerivedMethod = methodInfo.DeclaringType.IsSubclassOf(typeof(PairMaker));
    return hasDerivedMethod;
  }

  private bool SwigDirectorIsReceiver() {
    return IsReceiver();
  }

  private int SwigDirectorGetAddressCount() {
    return GetAddressCount();
  }

  private global::System.IntPtr SwigDirectorGetAddress(int index) {
    return  XString.getCPtr(GetAddress(index)).Handle ;
  }

  private ushort SwigDirectorGetPort() {
    return GetPort();
  }

  private void SwigDirectorUpdate() {
    Update();
  }

  private bool SwigDirectorIsReadyToConnect() {
    return IsReadyToConnect();
  }

  private int SwigDirectorGetLocalKey() {
    return GetLocalKey();
  }

  private int SwigDirectorGetRemoteKey() {
    return GetRemoteKey();
  }

  public delegate bool SwigDelegatePairMaker_0();
  public delegate int SwigDelegatePairMaker_1();
  public delegate global::System.IntPtr SwigDelegatePairMaker_2(int index);
  public delegate ushort SwigDelegatePairMaker_3();
  public delegate void SwigDelegatePairMaker_4();
  public delegate bool SwigDelegatePairMaker_5();
  public delegate int SwigDelegatePairMaker_6();
  public delegate int SwigDelegatePairMaker_7();

  private SwigDelegatePairMaker_0 swigDelegate0;
  private SwigDelegatePairMaker_1 swigDelegate1;
  private SwigDelegatePairMaker_2 swigDelegate2;
  private SwigDelegatePairMaker_3 swigDelegate3;
  private SwigDelegatePairMaker_4 swigDelegate4;
  private SwigDelegatePairMaker_5 swigDelegate5;
  private SwigDelegatePairMaker_6 swigDelegate6;
  private SwigDelegatePairMaker_7 swigDelegate7;

  private static global::System.Type[] swigMethodTypes0 = new global::System.Type[] {  };
  private static global::System.Type[] swigMethodTypes1 = new global::System.Type[] {  };
  private static global::System.Type[] swigMethodTypes2 = new global::System.Type[] { typeof(int) };
  private static global::System.Type[] swigMethodTypes3 = new global::System.Type[] {  };
  private static global::System.Type[] swigMethodTypes4 = new global::System.Type[] {  };
  private static global::System.Type[] swigMethodTypes5 = new global::System.Type[] {  };
  private static global::System.Type[] swigMethodTypes6 = new global::System.Type[] {  };
  private static global::System.Type[] swigMethodTypes7 = new global::System.Type[] {  };
}

}
