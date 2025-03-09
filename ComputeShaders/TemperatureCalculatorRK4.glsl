#[compute]

#version 450

layout(local_size_x=32,local_size_y=32,local_size_z=1)in;

layout(set=0,binding=0,std430)buffer LocalTemp{
    float data[];
}local_temp;

layout(set=0,binding=1,std430)buffer NeighborIndex{
    uvec4 data[];
}neighbor_index;

layout(set=0,binding=2,std430)buffer DeltaTime{
    float timestamp;
}delta_time;

const float alpha=1e-4F;
const uint grid_size=128;
const float dx2_inv=(grid_size-1)<<1;

float computeHeatEquation(uint id,float temp_self,float temp_left,float temp_right,float temp_bottom,float temp_top){
    float d2x=(temp_left+temp_right-2.*temp_self)*dx2_inv;
    float d2y=(temp_bottom+temp_top-2.*temp_self)*dx2_inv;
    return alpha*(d2x+d2y);
}

void main(){
    uint x=gl_GlobalInvocationID.x;
    uint y=gl_GlobalInvocationID.y;
    uint z=gl_GlobalInvocationID.z;
    
    uint id=gl_GlobalInvocationID.x+
    gl_GlobalInvocationID.y*(gl_NumWorkGroups.x*gl_WorkGroupSize.x)+
    gl_GlobalInvocationID.z*(gl_NumWorkGroups.x*gl_WorkGroupSize.x)*
    (gl_NumWorkGroups.y*gl_WorkGroupSize.y);
    
    float dt=delta_time.timestamp;
    // float dt=0.4f;
    
    uvec4 neighbors=neighbor_index.data[id];
    uint left=neighbors.x;
    uint right=neighbors.y;
    uint bottom=neighbors.z;
    uint top=neighbors.w;
    
    //temp for deriv
    float T0=local_temp.data[id];
    
    // rk4
    float k1=computeHeatEquation(id,T0,
        local_temp.data[left],local_temp.data[right],
        local_temp.data[bottom],local_temp.data[top]
    );
    
    float T_k2=T0+.5*dt*k1;
    float k2=computeHeatEquation(id,T_k2,
        local_temp.data[left]+.5*dt*k1,
        local_temp.data[right]+.5*dt*k1,
        local_temp.data[bottom]+.5*dt*k1,
        local_temp.data[top]+.5*dt*k1
    );
    
    float T_k3=T0+.5*dt*k2;
    float k3=computeHeatEquation(id,T_k3,
        local_temp.data[left]+.5*dt*k2,
        local_temp.data[right]+.5*dt*k2,
        local_temp.data[bottom]+.5*dt*k2,
        local_temp.data[top]+.5*dt*k2
    );
    
    float T_k4=T0+dt*k3;
    float k4=computeHeatEquation(id,T_k4,
        local_temp.data[left]+dt*k3,
        local_temp.data[right]+dt*k3,
        local_temp.data[bottom]+dt*k3,
        local_temp.data[top]+dt*k3
    );
    
    local_temp.data[id]+=dt*(k1+2.*k2+2.*k3+k4)/6.;
}