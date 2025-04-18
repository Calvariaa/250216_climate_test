#[compute]

#version 450

layout(local_size_x=32,local_size_y=32,local_size_z=1)in;

layout(set=0,binding=0,r32f)uniform restrict image2D temperature;

layout(set=0,binding=1,std430)restrict readonly buffer NeighborIndex{
    uvec4 data[];
}neighbor_index;

layout(set=0,binding=2,std430)restrict buffer DeltaTime{
    float timestamp;
}delta_time;

// layout(set=0,binding=3,std430)restrict buffer TemperatureTest{
    //     float data[];
// }temp_test;

layout(push_constant,std430)uniform Params{
    int face_length;
    float global_alpha;
}const_param;

// 节点间距 dx = 0.0025
float dx2_inv=float(400<<1);

// id to texture uv

uvec2 id_to_uv(uint id){
    uint face_size=const_param.face_length;
    
    uint face_id=id/(face_size*face_size);
    
    uint local_id=id%(face_size*face_size);
    return uvec2(
        face_id*face_size+(local_id%face_size),
        local_id/face_size
    );
}

float computeHeatEquation(uint id,float temp_self,float temp_left,float temp_right,float temp_bottom,float temp_top){
    float d2x=(temp_left+temp_right-2.*temp_self)*dx2_inv;
    float d2y=(temp_bottom+temp_top-2.*temp_self)*dx2_inv;
    return const_param.global_alpha*(d2x+d2y);
}

// https://i.imgur.com/RIlJM32.png
// https://zhuanlan.zhihu.com/p/8616433050
void main(){
    uint id=gl_GlobalInvocationID.x+
    gl_GlobalInvocationID.y*gl_WorkGroupSize.x*gl_NumWorkGroups.x+
    gl_GlobalInvocationID.z*gl_WorkGroupSize.x*gl_NumWorkGroups.x*gl_WorkGroupSize.y*gl_NumWorkGroups.y;
    
    if(id>const_param.face_length*const_param.face_length*6){
        return;
    }
    
    float dt=delta_time.timestamp;
    
    uvec4 neighbors=neighbor_index.data[id];
    
    uint top=neighbors.x;
    uint bottom=neighbors.y;
    uint left=neighbors.z;
    uint right=neighbors.w;
    
    // get id for texture
    uvec2 self_uv=id_to_uv(id);
    
    float T0=imageLoad(temperature,ivec2(self_uv)).r;
    
    // neighbors
    uvec2 left_uv=id_to_uv(left);
    uvec2 right_uv=id_to_uv(right);
    uvec2 bottom_uv=id_to_uv(bottom);
    uvec2 top_uv=id_to_uv(top);
    
    float temp_left=imageLoad(temperature,ivec2(left_uv)).r;
    float temp_right=imageLoad(temperature,ivec2(right_uv)).r;
    float temp_bottom=imageLoad(temperature,ivec2(bottom_uv)).r;
    float temp_top=imageLoad(temperature,ivec2(top_uv)).r;
    
    // RK4
    float k1=computeHeatEquation(id,T0,temp_left,temp_right,temp_bottom,temp_top);
    
    float T_k2=T0+.5*dt*k1;
    float k2_temp_left=imageLoad(temperature,ivec2(left_uv)).r+.5*dt*k1;
    float k2_temp_right=imageLoad(temperature,ivec2(right_uv)).r+.5*dt*k1;
    float k2_temp_bottom=imageLoad(temperature,ivec2(bottom_uv)).r+.5*dt*k1;
    float k2_temp_top=imageLoad(temperature,ivec2(top_uv)).r+.5*dt*k1;
    float k2=computeHeatEquation(id,T_k2,k2_temp_left,k2_temp_right,k2_temp_bottom,k2_temp_top);
    
    float T_k3=T0+.5*dt*k2;
    float k3_temp_left=imageLoad(temperature,ivec2(left_uv)).r+.5*dt*k2;
    float k3_temp_right=imageLoad(temperature,ivec2(right_uv)).r+.5*dt*k2;
    float k3_temp_bottom=imageLoad(temperature,ivec2(bottom_uv)).r+.5*dt*k2;
    float k3_temp_top=imageLoad(temperature,ivec2(top_uv)).r+.5*dt*k2;
    float k3=computeHeatEquation(id,T_k3,k3_temp_left,k3_temp_right,k3_temp_bottom,k3_temp_top);
    
    float T_k4=T0+dt*k3;
    float k4_temp_left=imageLoad(temperature,ivec2(left_uv)).r+dt*k3;
    float k4_temp_right=imageLoad(temperature,ivec2(right_uv)).r+dt*k3;
    float k4_temp_bottom=imageLoad(temperature,ivec2(bottom_uv)).r+dt*k3;
    float k4_temp_top=imageLoad(temperature,ivec2(top_uv)).r+dt*k3;
    float k4=computeHeatEquation(id,T_k4,k4_temp_left,k4_temp_right,k4_temp_bottom,k4_temp_top);
    
    float final_temp=T0+dt*(k1+2.*k2+2.*k3+k4)/6.;
    
    // if(id>=const_param.face_length*const_param.face_length*0&&id<const_param.face_length*const_param.face_length*1){
        //         final_temp=120.;
    // }
    
    // 面的上边
    // if(id/(const_param.face_length*const_param.face_length)==3u&&((id%(const_param.face_length*const_param.face_length))/const_param.face_length)==0u){
        //     final_temp=120.;
    // }
    
    // 面的左边
    // if(id/(const_param.face_length*const_param.face_length)==0u&&id%(const_param.face_length)==0u){
        //     final_temp=120.;
    // }
    
    // 面的下边
    // if(id/(const_param.face_length*const_param.face_length)==0u&&id<const_param.face_length*const_param.face_length-12){
        //     final_temp=120.;
    // }
    
    // 面的右边
    // if(id/(const_param.face_length*const_param.face_length)==0u&&id%(const_param.face_length)==(const_param.face_length-1u)){
        //     final_temp=120.;
    // }
    
    // else{
        //         final_temp=0.;
    // }
    
    // if (self_uv.x == 30)
    //     final_temp = 120.0;
    // final_temp=id%240-120;
    imageStore(temperature,ivec2(self_uv),vec4(final_temp,final_temp,final_temp,1.));
    
    // temp_test.data[id]=imageLoad(temperature,ivec2(self_uv)).r;
}