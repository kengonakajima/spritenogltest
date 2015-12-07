using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class Pool<T> where T : new() {
    Dictionary<uint,T> dict;
    public Pool() {
        dict = new Dictionary<uint,T>();
    }
    public void set(uint id, T t) {
        dict[id] = t;
    }
    public T get(uint id) {
        if(dict.ContainsKey(id)) {
            return dict[id];
        } else {
            return default(T);
        }
    }
    public T alloc(uint id) {
        T t = new T();
        FieldInfo fi = t.GetType().GetField( "id");
        fi.SetValue(t,id);
        set(id,t);
        return t;
    }
    public void del(uint id) {
        dict.Remove(id);
    }
    public T ensure(uint id) {
        T t = get(id);
        if(t==null) {
            t = alloc(id);
        }
        return t;
    }    
}

/*
// 汎用のID>ポインタ検索
template <class Obj> class ObjectPool {
public:
    std::unordered_map<unsigned int,Obj*> idmap;
    ObjectPool() {};
    void set(unsigned int id, Obj *ptr ) {
        idmap[id] = ptr;
    }
    Obj *get(unsigned int id) {
        if( idmap.find(id) == idmap.end() ) return NULL; else return idmap[id];
    };
    Obj *alloc(unsigned int id) {
        Obj *ptr = new Obj();
        ptr->id = id;
        set( id, ptr );
        return ptr;
    }
    int del(unsigned int id) {
        int n = idmap.erase(id);
        return n;
    }    
    Obj *ensure(unsigned int id) {
        Obj *ptr = get(id);
        if(!ptr) {
            ptr = alloc(id);
        }
        return ptr;
    }
    Obj *ensure(unsigned int id, int arg0, int arg1 ) {
        Obj *ptr = get(id);
        if(!ptr) {
            ptr = new Obj(arg0, arg1);
            ptr->id = id;
            set(id,ptr);
        }
        return ptr;
    }
};

#endif
*/
